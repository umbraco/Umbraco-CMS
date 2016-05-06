using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    public abstract class AbstractConfigCheck : HealthCheck
    {
        /// <summary>
        /// Gets the config file path.
        /// </summary>
        public abstract string FilePath { get; }

        /// <summary>
        /// Gets XPath statement to the config element to check.
        /// </summary>
        public abstract string XPath { get; }

        /// <summary>
        /// Gets the values to compare against.
        /// </summary>
        public abstract IEnumerable<AcceptableConfiguration> Values { get; }

        /// <summary>
        /// Gets the current value
        /// </summary>
        public string CurrentValue { get; set; }

        /// <summary>
        /// Gets the comparison type for checking the value.
        /// </summary>
        public abstract ValueComparisonType ValueComparisonType { get; }

        protected AbstractConfigCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        private string FileName
        {
            get { return Path.GetFileName(FilePath); }
        }

        /// <summary>
        /// Gets the absolute file path.
        /// </summary>
        private string AbsoluteFilePath
        {
            get { return HttpContext.Current.Server.MapPath(FilePath); }
        }

        /// <summary>
        /// Gets the message for when the check has succeeded.
        /// </summary>
        public virtual string CheckSuccessMessage
        {
            get { return "Node <strong>{1}</strong> passed check successfully."; }
        }

        /// <summary>
        /// Gets the message for when the check has failed.
        /// </summary>
        public virtual string CheckErrorMessage
        {
            get
            {
                return ValueComparisonType == ValueComparisonType.ShouldEqual
                    ? "Expected value <strong>{2}</strong> for the node <strong>{1}</strong> in config <strong>{0}</strong>, but found <strong>{3}</strong>"
                    : "Found unexpected value <strong>{2}</strong> for the node <strong>{1}</strong> in config <strong>{0}</strong>";
            }
        }

        /// <summary>
        /// Gets the rectify success message.
        /// </summary>
        public virtual string RectifySuccessMessage
        {
            get { return "Node <strong>{1}</strong> rectified successfully."; }
        }

        /// <summary>
        /// Gets a value indicating whether this check can be rectified automatically.
        /// </summary>
        public virtual bool CanRectify
        {
            get { return ValueComparisonType == ValueComparisonType.ShouldEqual; }
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(AbsoluteFilePath);

            var xmlNode = xmlDocument.SelectSingleNode(XPath);
            if (xmlNode == null)
            {
                var message = string.Format("Unable to find node <strong>{1}</strong> in config file <strong>{0}</strong>", FileName, XPath);
                return new[] { new HealthCheckStatus(message) { ResultType = StatusResultType.Error } };
            }

            var currentValue = xmlNode.Value ?? xmlNode.InnerText;
            CurrentValue = currentValue;

            var valueFound = Values.Any(value => string.Equals(currentValue, value.Value, StringComparison.InvariantCultureIgnoreCase));
            if (ValueComparisonType == ValueComparisonType.ShouldEqual && valueFound || ValueComparisonType == ValueComparisonType.ShouldNotEqual && valueFound)
            {
                var message = string.Format(CheckSuccessMessage, FileName, XPath, Values, currentValue);
                return new[] { new HealthCheckStatus(message) { ResultType = StatusResultType.Success } };
            }

            // Declare the action for rectifying the config value
            var rectifyAction = new HealthCheckAction("rectify", Id) { Name = "Rectify" };

            var resultMessage = string.Format(CheckErrorMessage, FileName, XPath, Values, currentValue);
            return new[]
            {
                new HealthCheckStatus(resultMessage)
                {
                    ResultType = StatusResultType.Error,
                    Actions = CanRectify ? new[] { rectifyAction } : new HealthCheckAction[0]
                }
            };
        }

        /// <summary>
        /// Rectifies this check.
        /// </summary>
        /// <returns></returns>
        public virtual HealthCheckStatus Rectify()
        {
            if (ValueComparisonType == ValueComparisonType.ShouldNotEqual)
                throw new InvalidOperationException("Cannot rectify a check with a value comparison type of ShouldNotEqual.");

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(AbsoluteFilePath);

            var node = xmlDocument.SelectSingleNode(XPath);
            if (node == null)
            {
                var message = string.Format("Unable to find node <strong>{1}</strong> in config file <strong>{0}</strong>", FileName, XPath);
                return new HealthCheckStatus(message) { ResultType = StatusResultType.Error };
            }

            var newValue = Values.First(v => v.IsRecommended).Value;
            if (node.NodeType == XmlNodeType.Element)
                node.InnerText = newValue;
            else
                node.Value = newValue;

            xmlDocument.Save(AbsoluteFilePath);

            var resultMessage = string.Format(RectifySuccessMessage, FileName, XPath, Values);
            return new HealthCheckStatus(resultMessage) { ResultType = StatusResultType.Success };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return Rectify();
        }
    }
}