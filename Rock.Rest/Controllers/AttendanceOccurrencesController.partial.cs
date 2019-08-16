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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AttendanceOccurrencesController
    {
        /// <summary>
        /// Gets all the occurrences for a group for the selected dates, location and schedule.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/AttendanceOccurrences/GetFutureGroupOccurrences" )]
        public List<AttendanceOccurrence> GetFutureGroupOccurrences( int groupId, DateTime? toDateTime = null, string locationIds = null, string scheduleIds = null )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Configuration.ProxyCreationEnabled = false;
                var group = new GroupService( rockContext ).Get( groupId );

                return new AttendanceOccurrenceService( rockContext )
                    .GetFutureGroupOccurrences( group, toDateTime, locationIds, scheduleIds);
            }
        }

        /// <summary>
        /// Creates a new attendance occurrence for a group.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route("api/AttendanceOccurrences/CreateGroupOccurrence")]
        public AttendanceOccurrence CreateGroupOccurrence( int groupId, DateTime occurrenceDate, int? scheduleId = null, int? locationId = null )
        {
            return new AttendanceOccurrenceService( new RockContext () ).GetOrAdd( occurrenceDate, groupId, locationId, scheduleId );
        }

    }
}
