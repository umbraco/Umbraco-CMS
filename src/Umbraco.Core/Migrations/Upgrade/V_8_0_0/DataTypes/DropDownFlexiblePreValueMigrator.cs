﻿using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    class DropDownFlexiblePreValueMigrator : IPreValueMigrator
    {
        public bool CanMigrate(string editorAlias)
            => editorAlias == "Umbraco.DropDown.Flexible";

        public virtual string GetNewAlias(string editorAlias)
            => null;

        public object GetConfiguration(int dataTypeId, string editorAlias, Dictionary<string, PreValueDto> preValues)
        {
            var config = new DropDownFlexibleConfiguration();
            foreach (var preValue in preValues.Values)
            {
                if (preValue.Alias == "multiple")
                {
                    config.Multiple = (preValue.Value == "1");
                }
                else
                {
                    config.Items.Add(new ValueListConfiguration.ValueListItem { Id = preValue.Id, Value = preValue.Value });
                }
            }
            return config;
        }
    }
}
