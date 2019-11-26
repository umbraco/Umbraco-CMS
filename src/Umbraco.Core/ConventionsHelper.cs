using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    public static class ConventionsHelper
    {
        public static Dictionary<string, PropertyType> GetStandardPropertyTypeStubs() =>
            new Dictionary<string, PropertyType>
            {
                {
                    Constants.Conventions.Member.Comments,
                    new PropertyType(Constants.PropertyEditors.Aliases.TextArea, ValueStorageType.Ntext, true,
                        Constants.Conventions.Member.Comments)
                    {
                        Name = Constants.Conventions.Member.CommentsLabel
                    }
                },
                {
                    Constants.Conventions.Member.FailedPasswordAttempts,
                    new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Integer, true,
                        Constants.Conventions.Member.FailedPasswordAttempts)
                    {
                        Name = Constants.Conventions.Member.FailedPasswordAttemptsLabel,
                        DataTypeId = Constants.DataTypes.LabelInt
                    }
                },
                {
                    Constants.Conventions.Member.IsApproved,
                    new PropertyType(Constants.PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, true,
                        Constants.Conventions.Member.IsApproved)
                    {
                        Name = Constants.Conventions.Member.IsApprovedLabel
                    }
                },
                {
                    Constants.Conventions.Member.IsLockedOut,
                    new PropertyType(Constants.PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, true,
                        Constants.Conventions.Member.IsLockedOut)
                    {
                        Name = Constants.Conventions.Member.IsLockedOutLabel
                    }
                },
                {
                    Constants.Conventions.Member.LastLockoutDate,
                    new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Date, true,
                        Constants.Conventions.Member.LastLockoutDate)
                    {
                        Name = Constants.Conventions.Member.LastLockoutDateLabel,
                        DataTypeId = Constants.DataTypes.LabelDateTime
                    }
                },
                {
                    Constants.Conventions.Member.LastLoginDate,
                    new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Date, true,
                        Constants.Conventions.Member.LastLoginDate)
                    {
                        Name = Constants.Conventions.Member.LastLoginDateLabel,
                        DataTypeId = Constants.DataTypes.LabelDateTime
                    }
                },
                {
                    Constants.Conventions.Member.LastPasswordChangeDate,
                    new PropertyType(Constants.PropertyEditors.Aliases.Label, ValueStorageType.Date, true,
                        Constants.Conventions.Member.LastPasswordChangeDate)
                    {
                        Name = Constants.Conventions.Member.LastPasswordChangeDateLabel,
                        DataTypeId = Constants.DataTypes.LabelDateTime
                    }
                }
            };
    }
}
