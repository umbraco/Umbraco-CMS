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
            RenameDataType(Constants.PropertyEditors.Aliases.ContentPicker + "2", Constants.PropertyEditors.Aliases.ContentPicker);
            RenameDataType(Constants.PropertyEditors.Aliases.MediaPicker + "2", Constants.PropertyEditors.Aliases.MediaPicker);
            RenameDataType(Constants.PropertyEditors.Aliases.MemberPicker + "2", Constants.PropertyEditors.Aliases.MemberPicker);
            RenameDataType(Constants.PropertyEditors.Aliases.MultiNodeTreePicker + "2", Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
            RenameDataType("Umbraco.TextboxMultiple", Constants.PropertyEditors.Aliases.TextArea, false);
            RenameDataType("Umbraco.Textbox", Constants.PropertyEditors.Aliases.TextBox, false);
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
                    throw new InvalidOperationException($"Cannot rename datatype alias \"{fromAlias}\" to \"{toAlias}\" because the target alias is already used.");
            }

            Database.Execute(Sql()
                .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, toAlias))
                .Where<DataTypeDto>(x => x.EditorAlias == fromAlias));
        }
    }
}
