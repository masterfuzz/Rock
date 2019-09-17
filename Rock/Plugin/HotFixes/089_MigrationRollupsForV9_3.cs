﻿// <copyright>
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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 89, "1.9.0" )]
    public class MigrationRollupsForV9_3 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            FixMotivatorsPositionalSummaryText();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// NA: Fix Typo in the Summary text (attribute) of the Motivators Theme (DefinedType) for Positional (DefinedValue)
        /// </summary>
        private void FixMotivatorsPositionalSummaryText()
        {
            Sql( @"
                DECLARE @DefinedValueId int
                SET @DefinedValueId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '84322020-4E27-44EF-88F2-EAFDB7286A01')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '07E85FA1-8F86-4414-8DC3-43D303C55457')

                IF @DefinedValueId IS NOT NULL AND @AttributeId IS NOT NULL
                BEGIN

                    UPDATE [AttributeValue] 
                    SET [Value] = REPLACE( [Value],'motivators int this', 'motivators in this' ) 
                    WHERE [AttributeId] = @AttributeId AND [EntityId] = @DefinedValueId

                END" );
        }
    }
}
