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
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Modify the data model to rename SystemEmail to SystemCommunication.
    /// </summary>
    public partial class AddSystemCommunication : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            TableChangesUp();
            PagesAndBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            TableChangesDown();
            PagesAndBlocksDown();
        }

        private void TableChangesUp()
        {
            // Changes for: System Email
            RenameTable( name: "SystemEmail", newName: "SystemCommunication" );

            AddColumn( "SystemCommunication", "IsActive", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "SystemCommunication", "SMSMessage", c => c.String( nullable: true ) );
            AddColumn( "SystemCommunication", "SMSFromDefinedValueId ", c => c.Int() );
            AddColumn( "SystemCommunication", "PushTitle", c => c.String( maxLength: 100 ) );
            AddColumn( "SystemCommunication", "PushMessage", c => c.String() );
            AddColumn( "SystemCommunication", "PushSound", c => c.String( maxLength: 100 ) );

            RenameKey( "FK_dbo.SystemEmail_dbo.Category_CategoryId", "FK_dbo.SystemCommunication_dbo.Category_CategoryId" );
            RenameKey( "FK_dbo.SystemEmail_dbo.PersonAlias_CreatedByPersonAliasId", "FK_dbo.SystemCommunication_dbo.PersonAlias_CreatedByPersonAliasId" );
            RenameKey( "FK_dbo.SystemEmail_dbo.PersonAlias_ModifiedByPersonAliasId", "FK_dbo.SystemCommunication_dbo.PersonAlias_ModifiedByPersonAliasId" );
            RenameKey( "PK_dbo.SystemEmail", "PK_dbo.SystemCommunication" );

            // Changes for: Groups
            RenameColumn( "GroupType", "ScheduleConfirmationSystemEmailId", "ScheduleConfirmationSystemCommunicationId" );
            RenameColumn( "GroupType", "ScheduleReminderSystemEmailId", "ScheduleReminderSystemCommunicationId" );
            RenameKey( "FK_dbo.GroupType_dbo.SystemEmail_ScheduleReminderSystemEmailId", "FK_dbo.GroupType_dbo.SystemCommunication_ScheduleReminderSystemCommunicationId" );
            RenameKey( "FK_dbo.GroupType_dbo.SystemEmail_ScheduleConfirmationSystemEmailId", "FK_dbo.GroupType_dbo.SystemCommunication_ScheduleConfirmationSystemCommunicationId" );
            RenameIndex( "GroupType", "IX_ScheduleReminderSystemEmailId", "IX_ScheduleReminderSystemCommunicationId" );
            RenameIndex( "GroupType", "IX_ScheduleConfirmationSystemEmailId", "IX_ScheduleConfirmationSystemCommunicationId" );
            AddColumn( "GroupType", "RSVPReminderSystemCommunicationId", c => c.Int() );
            AddColumn( "GroupType", "RSVPReminderOffsetDays", c => c.Int() );

            AddColumn( "Group", "RSVPReminderSystemCommunicationId", c => c.Int() );
            AddColumn( "Group", "RSVPReminderOffsetDays", c => c.Int() );

            RenameColumn( "GroupSync", "WelcomeSystemEmailId", "WelcomeSystemCommunicationId" );
            RenameColumn( "GroupSync", "ExitSystemEmailId", "ExitSystemCommunicationId" );
            RenameKey( "FK_dbo.GroupSync_dbo.SystemEmail_WelcomeSystemEmailId", "FK_dbo.GroupSync_dbo.SystemCommunication_WelcomeSystemCommunicationId" );
            RenameKey( "FK_dbo.GroupSync_dbo.SystemEmail_ExitSystemEmailId", "FK_dbo.GroupSync_dbo.SystemCommunication_ExitSystemCommunicationId" );
            RenameIndex( "GroupSync", "IX_WelcomeSystemEmailId", "IX_WelcomeSystemCommunicationId" );
            RenameIndex( "GroupSync", "IX_ExitSystemEmailId", "IX_ExitSystemCommunicationId" );

            // Changes for: Signature Documents
            RenameColumn( "SignatureDocumentTemplate", "InviteSystemEmailId", "InviteSystemCommunicationId" );
            RenameKey( "FK_dbo.SignatureDocumentTemplate_dbo.SystemEmail_InviteSystemEmailId", "FK_dbo.SignatureDocumentTemplate_dbo.SystemCommunication_InviteSystemCommunicationId" );
            RenameIndex( "SignatureDocumentTemplate", "IX_InviteSystemEmailId", "IX_InviteSystemCommunicationId" );

            // Changes for: Workflow
            RenameColumn( "WorkflowActionForm", "NotificationSystemEmailId", "NotificationSystemCommunicationId" );
            RenameKey( "FK_dbo.WorkflowActionForm_dbo.SystemEmail_NotificationSystemEmailId", "FK_dbo.WorkflowActionForm_dbo.SystemCommunication_NotificationSystemCommunicationId" );
            RenameIndex( "WorkflowActionForm", "IX_NotificationSystemEmailId", "IX_NotificationSystemCommunicationId" );
        }

        private void PagesAndBlocksUp()
        {
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAILS, "System Communications" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_DETAILS, "System Communication Details" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_COMMUNICATIONS, "System Communication Categories" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_SYSTEM_EMAILS, "System Communication Categories" );

            RenameBlock( SystemGuid.Block.SYSTEM_COMMUNICATION_LIST, "System Communication List" );

            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.SCHEDULE, "RSVP Confirmation", "", "", SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION );
        }

        private void TableChangesDown()
        {
            // Revert changes for: Groups
            DropColumn( "GroupType", "RSVPReminderSystemCommunicationId" );
            DropColumn( "GroupType", "RSVPReminderOffsetDays" );
            RenameColumn( "GroupType", "ScheduleConfirmationSystemCommunicationId", "ScheduleConfirmationSystemEmailId" );
            RenameColumn( "GroupType", "ScheduleReminderSystemCommunicationId", "ScheduleReminderSystemEmailId" );
            RenameKey( "FK_dbo.GroupType_dbo.SystemCommunication_ScheduleConfirmationSystemCommunicationId", "FK_dbo.GroupType_dbo.SystemEmail_ScheduleConfirmationSystemEmailId" );
            RenameKey( "FK_dbo.GroupType_dbo.SystemCommunication_ScheduleReminderSystemCommunicationId", "FK_dbo.GroupType_dbo.SystemEmail_ScheduleReminderSystemEmailId" );
            RenameIndex( "GroupType", "IX_ScheduleReminderSystemCommunicationId", "IX_ScheduleReminderSystemEmailId" );
            RenameIndex( "GroupType", "IX_ScheduleConfirmationSystemCommunicationId", "IX_ScheduleConfirmationSystemEmailId" );

            DropColumn( "Group", "RSVPReminderSystemCommunicationId" );
            DropColumn( "Group", "RSVPReminderOffsetDays" );

            RenameColumn( "GroupSync", "WelcomeSystemCommunicationId", "WelcomeSystemEmailId" );
            RenameColumn( "GroupSync", "ExitSystemCommunicationId", "ExitSystemEmailId" );
            RenameKey( "FK_dbo.GroupSync_dbo.SystemCommunication_WelcomeSystemCommunicationId", "FK_dbo.GroupSync_dbo.SystemEmail_WelcomeSystemEmailId" );
            RenameKey( "FK_dbo.GroupSync_dbo.SystemCommunication_ExitSystemCommunicationId", "FK_dbo.GroupSync_dbo.SystemEmail_ExitSystemEmailId" );
            RenameIndex( "GroupSync", "IX_WelcomeSystemCommunicationId", "IX_WelcomeSystemEmailId" );
            RenameIndex( "GroupSync", "IX_ExitSystemCommunicationId", "IX_ExitSystemEmailId" );

            // Revert changes for: Signature Documents
            RenameColumn( "SignatureDocumentTemplate", "InviteSystemCommunicationId", "InviteSystemEmailId" );
            RenameKey( "FK_dbo.SignatureDocumentTemplate_dbo.SystemCommunication_InviteSystemCommunicationId", "FK_dbo.SignatureDocumentTemplate_dbo.SystemEmail_InviteSystemEmailId" );
            RenameIndex( "SignatureDocumentTemplate", "IX_InviteSystemCommunicationId", "IX_InviteSystemEmailId" );

            // Revert changes for: Workflow
            RenameColumn( "WorkflowActionForm", "NotificationSystemCommunicationId", "NotificationSystemEmailId" );
            RenameKey( "FK_dbo.WorkflowActionForm_dbo.SystemCommunication_NotificationSystemCommunicationId", "FK_dbo.WorkflowActionForm_dbo.SystemEmail_NotificationSystemEmailId" );
            RenameIndex( "WorkflowActionForm", "IX_NotificationSystemCommunicationId", "IX_NotificationSystemEmailId" );

            // Revert changes for: System Email
            RenameTable( "SystemCommunication", "SystemEmail" );

            DropColumn( "SystemEmail", "IsActive" );
            DropColumn( "SystemEmail", "SMSMessage" );
            DropColumn( "SystemEmail", "SMSFromDefinedValueId" );
            DropColumn( "SystemEmail", "PushTitle" );
            DropColumn( "SystemEmail", "PushMessage" );
            DropColumn( "SystemEmail", "PushSound" );

            RenameKey( "FK_dbo.SystemCommunication_dbo.Category_CategoryId", "FK_dbo.SystemEmail_dbo.Category_CategoryId" );
            RenameKey( "FK_dbo.SystemCommunication_dbo.PersonAlias_CreatedByPersonAliasId", "FK_dbo.SystemEmail_dbo.PersonAlias_CreatedByPersonAliasId" );
            RenameKey( "FK_dbo.SystemCommunication_dbo.PersonAlias_ModifiedByPersonAliasId", "FK_dbo.SystemEmail_dbo.PersonAlias_ModifiedByPersonAliasId" );
            RenameKey( "PK_dbo.SystemCommunication", "PK_dbo.SystemEmail" );
        }

        private void PagesAndBlocksDown()
        {
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAILS, "System Emails" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_DETAILS, "System Email Details" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_COMMUNICATIONS, "System Email Categories" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_SYSTEM_EMAILS, "System Email Categories" );

            RockMigrationHelper.DeleteCategory( SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION );
        }

        /// <summary>
        /// Rename a primary key or foreign key constraint.
        /// </summary>
        /// <param name="fromName"></param>
        /// <param name="toName"></param>
        private void RenameKey( string fromName, string toName )
        {
            // Entity Framework 6.0 does not include a method to rename a primary key or foreign key constraint, so use a SQL Server stored procedure.
            Sql( $"sp_rename @objname = N'[{fromName}]', @newname = N'{toName}', @objType = N'OBJECT'" );
        }

        /// <summary>
        /// Rename a block.
        /// </summary>
        /// <param name="blockGuid"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void RenameBlock( string blockGuid, string newName )
        {
            Sql( $"UPDATE [Block] SET [Name] = '{newName}' WHERE [Guid] = '{blockGuid}'" );
        }
    }
}
