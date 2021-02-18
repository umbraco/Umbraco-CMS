using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class PropertyEditorsMigration : MigrationBase
    {
        public PropertyEditorsMigration(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            RenameDataType(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.ContentPicker2, Cms.Core.Constants.PropertyEditors.Aliases.ContentPicker);
            RenameDataType(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.MediaPicker2, Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker);
            RenameDataType(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.MemberPicker2, Cms.Core.Constants.PropertyEditors.Aliases.MemberPicker);
            RenameDataType(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.MultiNodeTreePicker2, Cms.Core.Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
            RenameDataType(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.TextboxMultiple, Cms.Core.Constants.PropertyEditors.Aliases.TextArea, false);
            RenameDataType(Cms.Core.Constants.PropertyEditors.Legacy.Aliases.Textbox, Cms.Core.Constants.PropertyEditors.Aliases.TextBox, false);
        }

        private void RenameDataType(string fromAlias, string toAlias, bool checkCollision = true)
        {
            if (checkCollision)
            {
                var oldCount = Database.ExecuteScalar<int>(Sql()
                    .SelectCount()
                    .From<DataTypeDto>()
                    .Where<DataTypeDto>(x => x.EditorAlias == toAlias));

                if (oldCount > 0)
                {
                    // If we throw it means that the upgrade will exit and cannot continue.
                    // This will occur if a v7 site has the old "Obsolete" property editors that are already named with the `toAlias` name.
                    // TODO: We should have an additional upgrade step when going from 7 -> 8 like we did with 6 -> 7 that shows a compatibility report,
                    // this would include this check and then we can provide users with information on what they should do (i.e. before upgrading to v8 they will
                    // need to migrate these old obsolete editors to non-obsolete editors)

                    throw new InvalidOperationException(
                        $"Cannot rename datatype alias \"{fromAlias}\" to \"{toAlias}\" because the target alias is already used." +
                        $"This is generally because when upgrading from a v7 to v8 site, the v7 site contains Data Types that reference old and already Obsolete " +
                        $"Property Editors. Before upgrading to v8, any Data Types using property editors that are named with the prefix '(Obsolete)' must be migrated " +
                        $"to the non-obsolete v7 property editors of the same type.");
                }

            }

            Database.Execute(Sql()
                .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, toAlias))
                .Where<DataTypeDto>(x => x.EditorAlias == fromAlias));
        }
    }
}
