// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays the details of the given RSVP occurrence.
    /// </summary>
    [DisplayName( "RSVP Occurrence Detail" )]
    [Category( "RSVP" )]
    [Description( "Shows detailed RSVP information for a specific occurrence datetime and allows editing RSVP details." )]

    //ToDo:  This should be the GUID of the Decline Reasons type which should probably be created in a migration.
    [DefinedTypeField(
        "DeclineReasonsType",
        Key = AttributeKey.DeclineReasonsType,
        DefaultValue = "F9FBD423-2832-48AA-8C33-95DFA6878BEC" )]

    [DefinedValueField(
        "MapStyle",
        Key = AttributeKey.MapStyle,
        Description = "The style of maps to use for selecting locations.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK,
        Order = 3 )]

    public partial class RSVPDetail : RockBlock
    {
        #region Keys

        protected static class AttributeKey
        {
            public const string DeclineReasonsType = "DeclineReasonsType";
            public const string MapStyle = "MapStyle";
        }

        protected static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string OccurrenceId = "OccurrenceId";
            public const string OccurrenceDate = "OccurrenceDate";
        }

        protected static class PageLabels
        {
            public const string MemberLocationTabTitle = "Member Location";
            public const string OtherLocationTabTitle = "Other Location";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tracks the currently active "Location Type" tab (Member Location or Other Location).
        /// </summary>
        private string LocationTypeTab { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            GetAvailableDeclineReasons();

            if ( !Page.IsPostBack )
            {
                rcblAvailableDeclineReasons.DataSource = _allDeclineReasons;
                rcblAvailableDeclineReasons.DataBind();
            }
        }

        /// <summary>
        /// Refreshes the block display in case the block settings are changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var newLocationId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            if ( newLocationId != null )
            {
                // If a new location was created, pass the value to the page to show it after reloading.
                NavigateToCurrentPageReference( new Dictionary<string, string> { { PageParameterKey.OccurrenceId, newLocationId.Value.ToString() } } );
                return;
            }
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( !Page.IsPostBack )
            {
                if ( groupId == null )
                {
                    NavigateToParentPage();
                }
                else
                {
                    var rockContext = new RockContext();
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    lHeading.Text = "RSVP Detail " + group.Name;

                    int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
                    if ( occurrenceId == null )
                    {
                        occurrenceId = 0;
                    }

                    // Display Occurrence
                    ShowDetails( rockContext, occurrenceId.Value, group );
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            LocationTypeTab = ViewState["LocationTypeTab"] as string ?? PageLabels.MemberLocationTabTitle;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["LocationTypeTab"] = LocationTypeTab;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSelectLocation control.
        /// </summary>
        protected void lbSelectLocation_Click(object sender, EventArgs e)
        {
            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                // If the query string is 0, check to see if a new occurrence was already created.
                occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            }
            if ( occurrenceId == null )
            {
                occurrenceId = 0;
            }

            using ( var rockContext = new RockContext() )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                var group = new GroupService( rockContext ).Get( groupId.Value );
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get(occurrenceId.Value);
                SetupGroupLocationOptions( group, occurrence.Location, rockContext );
            }

            ShowDialog( "Locations" );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgLocations_OkClick( object sender, EventArgs e )
        {
            if ( LocationTypeTab.Equals( PageLabels.MemberLocationTabTitle ) )
            {
                if ( ddlMember.SelectedValue != null )
                {
                    var ids = ddlMember.SelectedValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList().ToArray();
                    if ( ids.Length == 2 )
                    {
                        int locationId = ids[0];
                        hfLocationId.Value = locationId.ToString();
                        lLocationEdit.Text = ddlMember.SelectedItem.Text;
                    }
                }
            }
            else
            {
                if ( locpGroupLocation.Location != null )
                {
                    var selectedLocation = locpGroupLocation.Location;
                    hfLocationId.Value = selectedLocation.Id.ToString();
                    lLocationEdit.Text = selectedLocation.ToString();
                }
            }

            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the lbProperty control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLocationType_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                LocationTypeTab = lb.Text;
                BindLocationTypeRepeater();
            }

            ShowSelectedPane();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                if ( SaveRSVPData() )
                {
                    NavigateToParentPage( new Dictionary<string, string> { { PageParameterKey.GroupId, groupId.Value.ToString() } } );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            if ( groupId != null )
            {
                NavigateToParentPage( new Dictionary<string, string> { { PageParameterKey.GroupId, groupId.Value.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAttendees grid.
        /// </summary>
        protected void gAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                // Bind Decline Reason dropdown values.
                RockDropDownList rddlDeclineReason = e.Row.FindControl( "rddlDeclineReason" ) as RockDropDownList;
                rddlDeclineReason.DataSource = _availableDeclineReasons;
                rddlDeclineReason.DataBind();

                RockCheckBox rcbAccept = e.Row.FindControl( "rcbAccept" ) as RockCheckBox;
                RockCheckBox rcbDecline = e.Row.FindControl( "rcbDecline" ) as RockCheckBox;
                rcbAccept.InputAttributes.Add( "data-paired-checkbox", rcbDecline.ClientID );
                rcbDecline.InputAttributes.Add( "data-paired-checkbox", rcbAccept.ClientID );

                var rsvpData = ( RSVPAttendee ) e.Row.DataItem;
                if (rsvpData.DeclineReason.HasValue)
                {
                    try
                    {
                        rddlDeclineReason.SelectedValue = rsvpData.DeclineReason.ToString();
                    }
                    catch
                    {
                        // This call may fail if the decline reason has been removed (from the DefinedType or from the individual occurrence).  Ignored.
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancelOccurrence control.
        /// </summary>
        protected void lbCancelOccurrence_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbEditOccurrence control.
        /// </summary>
        protected void lbEditOccurrence_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbSaveOccurrence control.
        /// </summary>
        protected void lbSaveOccurrence_Click( object sender, EventArgs e )
        {
            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                // If the query string is 0, check to see if a new occurrence was already created.
                occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            }

            if ( ( occurrenceId != null) && ( occurrenceId != 0 ) )
            {
                UpdateExistingOccurrence( occurrenceId.Value );
            }
            else
            {
                CreateNewOccurrence();
            }

            pnlEdit.Visible = false;
            pnlDetails.Visible = true;
            pnlAttendees.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Stores the available Decline Reasons (filtered by the occurrence) for use in the grid drop down menus so they can be reused for binding multiple controls.
        /// </summary>
        private List<DefinedValue> _availableDeclineReasons;

        /// <summary>
        /// Gets the available Decline Reasons (filtered by the occurrence) for use in the grid drop down menus.
        /// </summary>
        /// <returns></returns>
        protected void GetAvailableDeclineReasons()
        {
            // If the collection is already initialized, this method is unnecessary.  Note that this means the collection property needs to be reset to null in order to refresh the values.
            if ( _availableDeclineReasons != null )
            {
                return;
            }

            GetAllDeclineReasons();
            List<DefinedValue> values = new List<DefinedValue>();
            int? occurrenceId = PageParameter(PageParameterKey.OccurrenceId).AsIntegerOrNull();
            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
            }

            if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
            {
                // No occurrence - just return an empty value for now.
                _availableDeclineReasons = values;
                _availableDeclineReasons.Insert( 0, new DefinedValue() { Id = 0, Value = "" } );
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get( occurrenceId.Value );
                if ( occurrence.ShowDeclineReasons )
                {
                    if ( string.IsNullOrWhiteSpace( occurrence.DeclineReasonValueIds ) )
                    {
                        // if ShowDeclineReasons is true and no Decline Reasons were selected, show all available reasons.
                        foreach ( var value in _allDeclineReasons )
                        {
                            values.Add( value );
                        }
                    }
                    else
                    {
                        // Filter values by occurrence setting.
                        List<int> selectedDeclineReasons = occurrence.DeclineReasonValueIds.SplitDelimitedValues().Select( int.Parse ).ToList();
                        foreach ( DefinedValue value in _allDeclineReasons )
                        {
                            if ( selectedDeclineReasons.Contains( value.Id ) )
                            {
                                values.Add( value );
                            }
                        }
                    }
                }
            }

            _availableDeclineReasons = values;
            _availableDeclineReasons.Insert( 0, new DefinedValue() { Id = 0, Value = "" } );
        }

        /// <summary>
        /// Stores All Decline Reasons, for use in the occurrence edit panel.
        /// </summary>
        private List<DefinedValue> _allDeclineReasons;

        /// <summary>
        /// Gets all of the the Decline Reasons, for use in the occurrence edit panel
        /// </summary>
        /// <returns></returns>
        protected void GetAllDeclineReasons()
        {
            // If the collection is already initialized, this method is unnecessary.
            if ( _allDeclineReasons != null)
            {
                return;
            }

            List<DefinedValue> values = new List<DefinedValue>();

            var declineReasonsDefinedType = DefinedTypeCache.Get( GetAttributeValue( AttributeKey.DeclineReasonsType ) );
            if ( declineReasonsDefinedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    values = new DefinedValueService( rockContext ).Queryable()
                        .Where( v => v.DefinedTypeId == declineReasonsDefinedType.Id )
                        .Where( v => v.IsActive )
                        .AsNoTracking().ToList();
                }
            }

            _allDeclineReasons = values;
        }

        /// <summary>
        /// Display the RSVP etails of a specified occurrence.
        /// </summary>
        /// <param name="rockContext">The DbContext.</param>
        /// <param name="occurrenceId">The ID of the occurrence to display.</param>
        /// <param name="group">The group the occurrence belongs to.</param>
        private void ShowDetails( RockContext rockContext, int occurrenceId, Group group )
        {
            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            if ( group.ScheduleId != null )
            {
                spSchedule.SetValue( group.ScheduleId );
            }

            if ( occurrenceId == 0 )
            {
                ShowNewOccurrence();
            }
            else
            {
                ShowExistingOccurrence( rockContext, occurrenceId, group );
            }
        }

        /// <summary>
        /// Displays the edit form for a new occurrence.
        /// </summary>
        private void ShowNewOccurrence()
        {
            pnlEdit.Visible = true;
            pnlDetails.Visible = false;
            pnlAttendees.Visible = false;
            string occurrenceDate = PageParameter( PageParameterKey.OccurrenceDate );
            if ( !string.IsNullOrWhiteSpace( occurrenceDate ) )
            {
                dpOccurrenceDate.SelectedDate = occurrenceDate.AsDateTime();
            }
        }

        /// <summary>
        /// Displays the detail form for an existing occurrence.
        /// </summary>
        /// <param name="rockContext">The DbContext.</param>
        /// <param name="occurrenceId">The ID of the occurrence to display.</param>
        /// <param name="group">The group the occurrence belongs to.</param>
        private void ShowExistingOccurrence( RockContext rockContext, int occurrenceId, Group group )
        {
            pnlEdit.Visible = false;
            pnlDetails.Visible = true;
            pnlAttendees.Visible = true;

            var groupType = new GroupTypeService( rockContext ).Get( group.GroupTypeId );
            var occurrence = new AttendanceOccurrenceService( rockContext ).Get( occurrenceId );
            lOccurrenceDate.Text = occurrence.OccurrenceDate.ToLocalTime().ToShortDateString();
            dpOccurrenceDate.SelectedDate = occurrence.OccurrenceDate;
            heAcceptMessage.Text = occurrence.AcceptConfirmationMessage;
            heDeclineMessage.Text = occurrence.DeclineConfirmationMessage;

            rcbShowDeclineReasons.Checked = occurrence.ShowDeclineReasons;
            List<int> selectedDeclineReasons = occurrence.DeclineReasonValueIds.SplitDelimitedValues().Select( int.Parse ).ToList();
            foreach ( int declineReasonId in selectedDeclineReasons )
            {
                foreach ( ListItem liItem in rcblAvailableDeclineReasons.Items )
                {
                    if ( liItem.Value == declineReasonId.ToString() )
                    {
                        liItem.Selected = true;
                    }
                }
            }

            if ( occurrence.LocationId.HasValue )
            {
                lLocation.Text = occurrence.Location.ToString();
                lLocationEdit.Text = occurrence.Location.ToString();
                hfLocationId.Value = occurrence.LocationId.ToString();
            }

            if ( occurrence.Schedule == null )
            {
                lSchedule.Visible = false;
            }
            else
            {
                lSchedule.Visible = true;
                if ( !string.IsNullOrWhiteSpace( occurrence.Schedule.Name ) )
                {
                    lSchedule.Text = occurrence.Schedule.Name;
                }
                else
                {
                    lSchedule.Text = occurrence.Schedule.EntityStringValue;
                }
                spSchedule.SetValue( occurrence.ScheduleId );
            }

            BindAttendeeGridAndChart();

        }

        /// <summary>
        /// Shows the edit panel for the occurrence.
        /// </summary>
        private void ShowEdit()
        {
            pnlEdit.Visible = true;
            pnlDetails.Visible = false;
            pnlAttendees.Visible = false;
        }

        /// <summary>
        /// Binds the grid and chart with attendee data.
        /// </summary>
        private void BindAttendeeGridAndChart()
        {
            using ( var rockContext = new RockContext() )
            {
                int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
                if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
                {
                    occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
                }

                if ( ( occurrenceId != null ) && ( occurrenceId != 0 ) )
                {
                    var occurrenceService = new AttendanceOccurrenceService( rockContext );
                    var occurrence = occurrenceService.Get( occurrenceId.Value );
                    gAttendees.ColumnsOfType<RockTemplateField>()
                        .First( c => c.HeaderText == "Decline Reason" )
                        .Visible = occurrence.ShowDeclineReasons;
                    gAttendees.ColumnsOfType<RockTemplateField>()
                        .First( c => c.HeaderText == "Decline Note" )
                        .Visible = occurrence.ShowDeclineReasons;
                }
    
                var attendees = GetAttendees( rockContext );
                int acceptCount = attendees.Where( a => a.Accept ).Count();
                int declineCount = attendees.Where( a => a.Decline ).Count();
                int noResponseCount = attendees.Count() - acceptCount - declineCount;
                RegisterDoughnutChartScript( acceptCount, declineCount, noResponseCount );

                _availableDeclineReasons = null;
                GetAvailableDeclineReasons();

                gAttendees.DataSource = attendees;
                gAttendees.DataBind();
            }
        }

        /// <summary>
        /// Gets the attendee data (for use in the grid and chart).
        /// </summary>
        /// <param name="rockContext">The RockContext</param>
        /// <returns>A list of <see cref="RSVPAttendee"/> objects representing the attendees of an occurrence.</returns>
        private List<RSVPAttendee> GetAttendees( RockContext rockContext )
        {
            List<RSVPAttendee> attendees = new List<RSVPAttendee>();
            List<int> existingAttendanceRecords = new List<int>();

            int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();
            if ( ( occurrenceId != null ) && ( occurrenceId != 0 ) )
            {
                // Add RSVP responses for anyone who has an attendance record, already.
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get( occurrenceId.Value );
                foreach ( var attendee in occurrence.Attendees )
                {
                    RSVPAttendee rsvp = new RSVPAttendee();
                    rsvp.PersonId = attendee.PersonAlias.PersonId;
                    rsvp.NickName = attendee.PersonAlias.Person.NickName;
                    rsvp.LastName = attendee.PersonAlias.Person.LastName;
                    rsvp.Accept = ( attendee.RSVP == Rock.Model.RSVP.Yes );
                    rsvp.Decline = ( attendee.RSVP == Rock.Model.RSVP.No );
                    rsvp.DeclineReason = attendee.DeclineReasonValueId;
                    rsvp.DeclineNote = attendee.Note;
                    attendees.Add( rsvp );
                    existingAttendanceRecords.Add( attendee.PersonAlias.PersonId );
                }
            }

            //// Add any existing active members not on that list
            //int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            //var groupMemberService = new GroupMemberService( rockContext );

            //var groupMembersWithoutAttendance = groupMemberService
            //    .Queryable( "Person" ).AsNoTracking()
            //    .Where(m =>
            //       m.GroupId == groupId &&
            //       m.GroupMemberStatus == GroupMemberStatus.Active &&
            //       !existingAttendanceRecords.Contains(m.PersonId))
            //    .ToList();

            //foreach (var groupMember in groupMembersWithoutAttendance)
            //{
            //    RSVPAttendee rsvp = new RSVPAttendee();
            //    rsvp.PersonId = groupMember.PersonId;
            //    rsvp.NickName = groupMember.Person.NickName;
            //    rsvp.LastName = groupMember.Person.LastName;
            //    rsvp.Accept = false;
            //    rsvp.Decline = false;
            //    rsvp.DeclineReason = null;
            //    rsvp.DeclineNote = string.Empty;
            //    attendees.Add( rsvp );
            //}

            return attendees;
        }

        /// <summary>
        /// Registers the doughnut chart Chart.js script.
        /// </summary>
        private void RegisterDoughnutChartScript( int AcceptCount, int DeclineCount, int NoResponseCount )
        {
            string colors = "['#16C98D','#D4442E','#F3F3F3']";
            string rsvpData = "['"
                + AcceptCount.ToString() + "', '"
                + DeclineCount.ToString() + "', '"
                + NoResponseCount.ToString() + "']";

            string script = string.Format(
@"
var dnutCtx = $('#{0}')[0].getContext('2d');

var dnutChart = new Chart(dnutCtx, {{
    type: 'doughnut',
    data: {{
        labels: ['Accept', 'Decline', 'No Response'],
        datasets: [{{
            type: 'doughnut',
            data: {1},
            backgroundColor: {2}
        }}]
    }},
    options: {{
        responsive: true,
        legend: {{
            position: 'right',
            fullWidth: true
        }},
        cutoutPercentage: 75,
        animation: {{
			animateScale: true,
			animateRotate: true
		}}
    }}
}});",
                doughnutChartCanvas.ClientID,
                rsvpData,
                colors );

            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
            ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "groupSchedulerDoughnutChartScript", script, true );
        }

        /// <summary>
        /// Shows the selected pane.
        /// </summary>
        private void ShowSelectedPane()
        {
            if ( LocationTypeTab == PageLabels.MemberLocationTabTitle )
            {
                pnlMemberSelect.Visible = true;
                pnlLocationSelect.Visible = false;
            }
            else if ( LocationTypeTab == PageLabels.OtherLocationTabTitle )
            {
                pnlMemberSelect.Visible = false;
                pnlLocationSelect.Visible = true;
            }
        }

        /// <summary>
        /// Save RSVP response data from grid (to Attendance records).
        /// </summary>
        protected bool SaveRSVPData()
        {
            var attendees = new List<RSVPAttendee>();

            foreach ( GridViewRow row in gAttendees.Rows )
            {
                if ( row.RowType == DataControlRowType.DataRow )
                {
                    RockCheckBox rcbAccept = row.FindControl( "rcbAccept" ) as RockCheckBox;
                    RockCheckBox rcbDecline = row.FindControl( "rcbDecline" ) as RockCheckBox;
                    DataDropDownList rddlDeclineReason = row.FindControl( "rddlDeclineReason" ) as DataDropDownList;
                    RockTextBox tbDeclineNote = row.FindControl( "tbDeclineNote" ) as RockTextBox;
                    int declineReason = int.Parse( rddlDeclineReason.SelectedValue );
                    string declineNote = tbDeclineNote.Text;

                    attendees.Add(
                        new RSVPAttendee()
                        {
                            Accept = rcbAccept.Checked,
                            Decline = rcbDecline.Checked,
                            DeclineNote = declineNote,
                            DeclineReason = declineReason,
                            PersonId = ( int ) gAttendees.DataKeys[row.RowIndex].Value
                        }
                    );
                }
            }
            using ( var rockContext = new RockContext() )
            {
                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var attendanceService = new AttendanceService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var locationService = new LocationService( rockContext );

                AttendanceOccurrence occurrence = null;

                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                int? occurrenceId = PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();

                if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
                {
                    occurrenceId = hfNewOccurrenceId.Value.AsIntegerOrNull();
                    if ( ( occurrenceId == null ) || ( occurrenceId == 0 ) )
                    {
                        throw new Exception( "The AttendanceOccurrence does not exist." );
                    }
                }

                occurrence = occurrenceService.Get( occurrenceId.Value );

                var existingAttendees = occurrence.Attendees.ToList();

                foreach ( var attendee in attendees )
                {
                    var attendance = existingAttendees
                        .Where( a => a.PersonAlias.PersonId == attendee.PersonId )
                        .FirstOrDefault();

                    // This section is probably not needed, anymore.
                    if ( attendance == null )
                    {
                        int? personAliasId = personAliasService.GetPrimaryAliasId( attendee.PersonId );
                        if ( personAliasId.HasValue )
                        {
                            attendance = new Attendance();
                            attendance.PersonAliasId = personAliasId;
                            attendance.StartDateTime = occurrence.Schedule != null && occurrence.Schedule.HasSchedule() ? occurrence.OccurrenceDate.Date.Add( occurrence.Schedule.StartTimeOfDay ) : occurrence.OccurrenceDate;
                            occurrence.Attendees.Add( attendance );
                        }
                    }

                    if ( attendance != null )
                    {
                        if ( attendee.Accept )
                        {
                            attendance.RSVPDateTime = DateTime.Now;
                            attendance.RSVP = Rock.Model.RSVP.Yes;
                            attendance.Note = string.Empty;
                            attendance.DeclineReasonValueId = null;
                        }
                        else if ( attendee.Decline )
                        {
                            attendance.RSVPDateTime = DateTime.Now;
                            attendance.RSVP = Rock.Model.RSVP.No;
                            attendance.Note = attendee.DeclineNote;
                            if ( attendee.DeclineReason != 0 )
                            {
                                attendance.DeclineReasonValueId = attendee.DeclineReason;
                            }
                        }
                        else
                        {
                            attendance.RSVPDateTime = null;
                            attendance.RSVP = Rock.Model.RSVP.Unknown;
                            attendance.Note = string.Empty;
                            attendance.DeclineReasonValueId = null;
                        }
                    }
                }

                rockContext.SaveChanges();

                if ( occurrence.LocationId.HasValue )
                {
                    Rock.CheckIn.KioskLocationAttendance.Remove( occurrence.LocationId.Value );
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new AttendanceOccurrence record.
        /// </summary>
        private void CreateNewOccurrence()
        {
            using ( var rockContext = new RockContext() )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                var group = new GroupService( rockContext ).Get( groupId.Value );

                var occurrence = new AttendanceOccurrence();
                occurrence.GroupId = groupId.Value;

                //Create new occurrence.
                var attendanceOccurrence = new AttendanceOccurrence();
                attendanceOccurrence.GroupId = groupId;

                int? locationId = hfLocationId.Value.AsIntegerOrNull();
                occurrence.LocationId = locationId;

                attendanceOccurrence.ScheduleId = spSchedule.SelectedValueAsId();

                if (dpOccurrenceDate.SelectedDate.HasValue)
                {
                    occurrence.OccurrenceDate = dpOccurrenceDate.SelectedDate.Value;
                }

                var occurrenceService = new AttendanceOccurrenceService(rockContext);

                //If this occurrence has already been created, just use the existing one.
                var existingOccurrences = occurrenceService.Queryable()
                    .Where( o => o.GroupId == occurrence.GroupId)
                    .Where( o => o.OccurrenceDate == occurrence.OccurrenceDate)
                    .Where( o => o.ScheduleId == occurrence.ScheduleId)
                    .Where( o => o.LocationId == occurrence.LocationId)
                    .ToList();

                if ( existingOccurrences.Any() )
                {
                    occurrence = existingOccurrences.FirstOrDefault();
                }
                else
                {
                    occurrenceService.Add( occurrence );
                }

                occurrence.DeclineConfirmationMessage = heDeclineMessage.Text;
                occurrence.AcceptConfirmationMessage = heAcceptMessage.Text;
                occurrence.ShowDeclineReasons = rcbShowDeclineReasons.Checked;

                var selectedDeclineReasons = new List<string>();
                foreach ( ListItem listItem in rcblAvailableDeclineReasons.Items )
                {
                    if ( listItem.Selected )
                    {
                        selectedDeclineReasons.Add( listItem.Value );
                    }
                }
                occurrence.DeclineReasonValueIds = selectedDeclineReasons.AsDelimited( "," );

                rockContext.SaveChanges();

                hfNewOccurrenceId.Value = occurrence.Id.ToString();
                ShowDetails( rockContext, occurrence.Id, group );
            }
        }

        /// <summary>
        /// Saves changes to an existing AttendanceOccurrence record.
        /// </summary>
        private void UpdateExistingOccurrence( int occurrenceId )
        {
            using ( var rockContext = new RockContext() )
            {
                int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                var group = new GroupService( rockContext ).Get( groupId.Value );

                var occurrenceService = new AttendanceOccurrenceService( rockContext );
                var occurrence = occurrenceService.Get( occurrenceId );

                int? locationId = hfLocationId.Value.AsIntegerOrNull();
                occurrence.LocationId = locationId;

                occurrence.ScheduleId = spSchedule.SelectedValueAsId();

                if ( dpOccurrenceDate.SelectedDate.HasValue )
                {
                    occurrence.OccurrenceDate = dpOccurrenceDate.SelectedDate.Value;
                }

                occurrence.DeclineConfirmationMessage = heDeclineMessage.Text;
                occurrence.AcceptConfirmationMessage = heAcceptMessage.Text;
                occurrence.ShowDeclineReasons = rcbShowDeclineReasons.Checked;

                var selectedDeclineReasons = new List<string>();
                foreach ( ListItem listItem in rcblAvailableDeclineReasons.Items )
                {
                    if ( listItem.Selected )
                    {
                        selectedDeclineReasons.Add( listItem.Value );
                    }
                }
                occurrence.DeclineReasonValueIds = selectedDeclineReasons.AsDelimited( "," );

                rockContext.SaveChanges();

                ShowDetails(rockContext, occurrence.Id, group);
            }
        }

        /// <summary>
        /// Gets the CSS class for location tabs (used for switching active tabs).
        /// </summary>
        protected string GetLocationTabClass( object property )
        {
            if ( property.ToString() == LocationTypeTab )
            {
                return "active";
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "LOCATIONS":
                    dlgLocations.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "LOCATIONS":
                    dlgLocations.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Resets the location dialog.
        /// </summary>
        private void ResetLocationDialog()
        {
            locpGroupLocation.Location = null;
            //nbEditModeMessage.Text = string.Empty;
            //nbEditModeMessage.Visible = false;
        }

        /// <summary>
        /// Sets up the location selection options based on the group settings.
        /// </summary>
        /// <param name="group">The group that this occurrence belongs to.</param>
        /// <param name="occurrenceLocation">The existing location of the occurrence.  May be null.</param>
        /// <param name="rockContext">The DbContext.</param>
        private void SetupGroupLocationOptions( Group group, Location occurrenceLocation, RockContext rockContext )
        {
            ResetLocationDialog();
            var groupType = GroupTypeCache.Get( group.GroupTypeId );

            GroupLocationPickerMode groupTypeModes = groupType.LocationSelectionMode;
            if ( groupTypeModes == GroupLocationPickerMode.None )
            {
                // Hide group location
                return;
            }

            // Set the location picker modes allowed based on the group type's allowed modes
            LocationPickerMode modes = LocationPickerMode.None;
            if ( ( groupTypeModes & GroupLocationPickerMode.Named ) == GroupLocationPickerMode.Named )
            {
                modes = modes | LocationPickerMode.Named;
            }

            if ( ( groupTypeModes & GroupLocationPickerMode.Address ) == GroupLocationPickerMode.Address )
            {
                modes = modes | LocationPickerMode.Address;
            }

            if ( ( groupTypeModes & GroupLocationPickerMode.Point ) == GroupLocationPickerMode.Point )
            {
                modes = modes | LocationPickerMode.Point;
            }

            if ( ( groupTypeModes & GroupLocationPickerMode.Polygon ) == GroupLocationPickerMode.Polygon )
            {
                modes = modes | LocationPickerMode.Polygon;
            }

            bool displayMemberTab = ( groupTypeModes & GroupLocationPickerMode.GroupMember ) == GroupLocationPickerMode.GroupMember;
            bool displayOtherTab = modes != LocationPickerMode.None;

            ulNav.Visible = displayOtherTab && displayMemberTab;
            pnlMemberSelect.Visible = displayMemberTab;
            pnlLocationSelect.Visible = displayOtherTab && !displayMemberTab;

            if ( displayMemberTab )
            {
                var personService = new PersonService( rockContext );
                Guid previousLocationType = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid();

                ddlMember.Items.Add( new ListItem( string.Empty ) );
                foreach ( GroupMember member in new GroupMemberService( rockContext ).GetByGroupId( group.Id ) )
                {
                    foreach ( Group family in personService.GetFamilies( member.PersonId ) )
                    {
                        foreach ( GroupLocation familyGroupLocation in family.GroupLocations
                            .Where( l => l.IsMappedLocation && !l.GroupLocationTypeValue.Guid.Equals( previousLocationType ) ) )
                        {
                            ListItem li = new ListItem(
                                string.Format( "{0} {1} ({2})", member.Person.FullName, familyGroupLocation.GroupLocationTypeValue.Value, familyGroupLocation.Location.ToString() ),
                                string.Format( "{0}|{1}", familyGroupLocation.Location.Id, member.PersonId ) );

                            ddlMember.Items.Add( li );
                        }
                    }
                }
            }

            if ( displayOtherTab )
            {
                locpGroupLocation.AllowedPickerModes = modes;
                locpGroupLocation.SetBestPickerModeForLocation( null );
            }

            GroupLocation groupLocation = null;
            if ( occurrenceLocation != null )
            {
                groupLocation = group.GroupLocations
                    .Where( l => l.Location.Guid.Equals( occurrenceLocation.Guid ) )
                    .FirstOrDefault();
            }

            if ( groupLocation != null && groupLocation.Location != null )
            {
                if ( displayOtherTab )
                {
                    locpGroupLocation.SetBestPickerModeForLocation( groupLocation.Location );
                    locpGroupLocation.MapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();

                    if ( groupLocation.Location != null )
                    {
                        locpGroupLocation.Location = new LocationService( rockContext ).Get( groupLocation.Location.Id );
                    }
                }

                if ( displayMemberTab && ddlMember.Items.Count > 1 && groupLocation.GroupMemberPersonAliasId.HasValue )
                {
                    LocationTypeTab = PageLabels.MemberLocationTabTitle;
                    int? personId = new PersonAliasService( rockContext ).GetPersonId( groupLocation.GroupMemberPersonAliasId.Value );
                    if ( personId.HasValue )
                    {
                        ddlMember.SetValue( string.Format( "{0}|{1}", groupLocation.LocationId, personId.Value ) );
                    }
                }
                else if ( displayOtherTab )
                {
                    LocationTypeTab = PageLabels.OtherLocationTabTitle;
                }

                hfAddLocationGroupGuid.Value = occurrenceLocation.Guid.ToString();
            }
            else
            {
                hfAddLocationGroupGuid.Value = string.Empty;
                LocationTypeTab = ( displayMemberTab && ddlMember.Items.Count > 1 ) ? PageLabels.MemberLocationTabTitle : PageLabels.OtherLocationTabTitle;
            }

            BindLocationTypeRepeater();

            ShowSelectedPane();
            ShowDialog( "Locations", true );
        }

        /// <summary>
        /// Creates and binds the DataSource for rptLocationTypes conrol.
        /// </summary>
        private void BindLocationTypeRepeater()
        {
            rptLocationTypes.DataSource = new List<string> { PageLabels.MemberLocationTabTitle, PageLabels.OtherLocationTabTitle };
            rptLocationTypes.DataBind();
        }

        #endregion

        #region Helper Class

        [Serializable]
        public class RSVPAttendee
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the nick.
            /// </summary>
            /// <value>
            /// The name of the nick.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get { return NickName + " " + LastName; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RSVPAttendee"/> has accepted.
            /// </summary>
            /// <value>
            ///   <c>true</c> if accepted; otherwise, <c>false</c>.
            /// </value>
            public bool Accept { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RSVPAttendee"/> has declined.
            /// </summary>
            /// <value>
            ///   <c>true</c> if declined; otherwise, <c>false</c>.
            /// </value>
            public bool Decline { get; set; }

            public int? DeclineReason { get; set; }

            public string DeclineNote { get; set; }
        }

        #endregion

    }
}