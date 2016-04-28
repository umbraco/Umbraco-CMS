using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;

namespace Umbraco.Web.HealthCheck.Checks.Config {
    
    public abstract class AbstractConfigCheck : HealthCheck {

        #region Properties

        /// <summary>
        /// Gets the config file path.
        /// </summary>
        public abstract string FilePath { get; }

        /// <summary>
        /// Gets XPath statement to the config element to check.
        /// </summary>
        public abstract string XPath { get; }

        /// <summary>
        /// Gets the value to compare against.
        /// </summary>
        public abstract string Value { get; }

        /// <summary>
        /// Gets the comparison type for checking the value.
        /// </summary>
        public abstract ValueComparisonType ValueComparisonType { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        private string FileName {
            get {
                return Path.GetFileName(FilePath);
            }
        }

        /// <summary>
        /// Gets the absolute file path.
        /// </summary>
        private string AbsoluteFilePath {
            get {
                return HttpContext.Current.Server.MapPath(FilePath);
            }
        }

        /// <summary>
        /// Gets the message for when the check has succeeded.
        /// </summary>
        public virtual string CheckSuccessMessage {
            get { return "Node <strong>{1}</strong> passed check successfully."; }
        }

        /// <summary>
        /// Gets the message for when the check has failed.
        /// </summary>
        public virtual string CheckErrorMessage {
            get {
                return (ValueComparisonType == ValueComparisonType.ShouldEqual)
                    ? "Expected value <strong>{2}</strong> for the node <strong>{1}</strong> in config <strong>{0}</strong>, but found <strong>{3}</strong>"
                    : "Found unexpected value <strong>{2}</strong> for the node <strong>{1}</strong> in config <strong>{0}</strong>";
            }
        }

        /// <summary>
        /// Gets the rectify success message.
        /// </summary>
        public virtual string RectifySuccessMessage {
            get { return "Node <strong>{1}</strong> rectified successfully."; }
        }

        /// <summary>
        /// Gets a value indicating whether this check can be rectified automatically.
        /// </summary>
        public virtual bool CanRectify {
            get { return ValueComparisonType == ValueComparisonType.ShouldEqual; }
        }

        #endregion

        #region Constructors

        protected AbstractConfigCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext) { }

        #endregion

        #region Member methods

        public override IEnumerable<HealthCheckStatus> GetStatus() {

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(AbsoluteFilePath);
            
            XmlNode xmlNode = xmlDocument.SelectSingleNode(XPath);
            if (xmlNode == null) {
                return OneIsAFlock(new HealthCheckStatus {
                    ResultType = StatusResultType.Error,
                    Message = String.Format("Unable to find node <strong>{1}</strong> in config file <strong>{0}</strong>", FileName, XPath)
                });
            }

            string str = xmlNode.Value ?? xmlNode.InnerText;

            if (ValueComparisonType == ValueComparisonType.ShouldEqual && str == Value || ValueComparisonType == ValueComparisonType.ShouldNotEqual && str != Value) {
                return OneIsAFlock(new HealthCheckStatus {
                    ResultType = StatusResultType.Success,
                    Message = String.Format(CheckSuccessMessage, FileName, XPath, Value, str)
                });
            }

            // Declare the action for rectifying the config value (we don't really use the GUID for this check)
            HealthCheckAction rectifyAction = new HealthCheckAction("rectify", Guid.NewGuid()) {
                Name = "Rectify"
            };

            return OneIsAFlock(new HealthCheckStatus {
                ResultType = StatusResultType.Error,
                Message = String.Format(CheckErrorMessage, FileName, XPath, Value, str),
                Actions = CanRectify ? new [] { rectifyAction } : new HealthCheckAction[0]
            });

        }

        /// <summary>
        /// Rectifies this check.
        /// </summary>
        /// <returns></returns>
        public virtual HealthCheckStatus Rectify() {
            
            if (ValueComparisonType == ValueComparisonType.ShouldNotEqual)
                throw new InvalidOperationException("Cannot rectify a check with a value comparison type of ShouldNotEqual.");

            XmlDocument doc = new XmlDocument();
            doc.Load(AbsoluteFilePath);

            XmlNode node = doc.SelectSingleNode(XPath);
            if (node == null) {
                return new HealthCheckStatus {
                    ResultType = StatusResultType.Error,
                    Message = String.Format("Unable to find node <strong>{1}</strong> in config file <strong>{0}</strong>", FileName, XPath)
                };
            }

            node.Value = Value;
            doc.Save(AbsoluteFilePath);

            return new HealthCheckStatus {
                ResultType = StatusResultType.Success,
                Message = String.Format(RectifySuccessMessage, FileName, XPath, Value)
            };
        
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action) {
            return Rectify();
        }

        private IEnumerable<HealthCheckStatus> OneIsAFlock(HealthCheckStatus status) {
            return new [] {status};
        }

        #endregion

    }

}