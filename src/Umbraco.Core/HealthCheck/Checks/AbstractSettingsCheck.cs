using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks
{
    public abstract class AbstractSettingsCheck : HealthCheck
    {
        protected ILocalizedTextService TextService { get; }
        protected ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Gets key within the JSON to check, in the colon-delimited format
        /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1
        /// </summary>
        public abstract string ItemPath { get; }

        /// <summary>
        /// Gets the values to compare against.
        /// </summary>
        public abstract IEnumerable<AcceptableConfiguration> Values { get; }

        /// <summary>
        /// Gets the current value of the config setting
        /// </summary>
        public abstract string CurrentValue { get; }

        /// <summary>
        /// Gets the provided value
        /// </summary>
        public string ProvidedValue { get; set; }

        /// <summary>
        /// Gets the comparison type for checking the value.
        /// </summary>
        public abstract ValueComparisonType ValueComparisonType { get; }

        protected AbstractSettingsCheck(ILocalizedTextService textService, ILoggerFactory loggerFactory)
        {
            TextService = textService;
            LoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the message for when the check has succeeded.
        /// </summary>
        public virtual string CheckSuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/checkSuccessMessage", new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, ItemPath });
            }
        }

        /// <summary>
        /// Gets the message for when the check has failed.
        /// </summary>
        public virtual string CheckErrorMessage
        {
            get
            {
                return ValueComparisonType == ValueComparisonType.ShouldEqual
                    ? TextService.Localize("healthcheck/checkErrorMessageDifferentExpectedValue",
                        new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, ItemPath })
                    : TextService.Localize("healthcheck/checkErrorMessageUnexpectedValue",
                        new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, ItemPath });
            }
        }

        /// <summary>
        /// Gets the rectify success message.
        /// </summary>
        public virtual string RectifySuccessMessage
        {
            get
            {
                AcceptableConfiguration recommendedValue = Values.FirstOrDefault(v => v.IsRecommended);
                string rectifiedValue = recommendedValue != null ? recommendedValue.Value : ProvidedValue;
                return TextService.Localize("healthcheck/rectifySuccessMessage",
                    new[]
                    {
                        CurrentValue,
                        rectifiedValue,
                        ItemPath
                    });
            }
        }

        /// <summary>
        /// Gets a value indicating whether this check can be rectified automatically.
        /// </summary>
        public virtual bool CanRectify => ValueComparisonType == ValueComparisonType.ShouldEqual;

        /// <summary>
        /// Gets a value indicating whether this check can be rectified automatically if a value is provided.
        /// </summary>
        public virtual bool CanRectifyWithValue => ValueComparisonType == ValueComparisonType.ShouldNotEqual;

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            // update the successMessage with the CurrentValue
            var successMessage = string.Format(CheckSuccessMessage, ItemPath, Values, CurrentValue);
            bool valueFound = Values.Any(value => string.Equals(CurrentValue, value.Value, StringComparison.InvariantCultureIgnoreCase));

            if (ValueComparisonType == ValueComparisonType.ShouldEqual
                && valueFound || ValueComparisonType == ValueComparisonType.ShouldNotEqual
                && valueFound == false)
            {
                return new[]
                {
                    new HealthCheckStatus(successMessage)
                    {
                        ResultType = StatusResultType.Success
                    }
                };
            }

            // Declare the action for rectifying the config value
            var rectifyAction = new HealthCheckAction("rectify", Id)
            {
                Name = TextService.Localize("healthcheck/rectifyButton"),
                ValueRequired = CanRectifyWithValue
            };

            string resultMessage = string.Format(CheckErrorMessage, ItemPath, Values, CurrentValue);
            return new[]
            {
                new HealthCheckStatus(resultMessage)
                {
                    ResultType = StatusResultType.Error,
                    Actions = CanRectify || CanRectifyWithValue ? new[] { rectifyAction } : new HealthCheckAction[0]
                }
            };
        }

        /// <summary>
        /// Rectifies this check.
        /// </summary>
        /// <returns></returns>
        public virtual HealthCheckStatus Rectify(HealthCheckAction action)
        {
            if (ValueComparisonType == ValueComparisonType.ShouldNotEqual)
            {
                throw new InvalidOperationException(TextService.Localize("healthcheck/cannotRectifyShouldNotEqual"));
            }

            //TODO: show message instead of actually fixing config
            string recommendedValue = Values.First(v => v.IsRecommended).Value;
            string resultMessage = string.Format(RectifySuccessMessage, ItemPath, Values);
            return new HealthCheckStatus(resultMessage) { ResultType = StatusResultType.Success };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return Rectify(action);
        }
    }
}
