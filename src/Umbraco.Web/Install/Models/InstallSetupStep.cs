using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Install.Models
{
    /// <summary>
    /// Model to give to the front-end to collect the information for each step
    /// </summary>
    [DataContract(Name = "step", Namespace = "")]
    public abstract class InstallSetupStep<T> : InstallSetupStep
    {
        /// <summary>
        /// Defines the step model type on the server side so we can bind it
        /// </summary>
        [IgnoreDataMember]
        public override Type StepType
        {
            get { return typeof(T); }
        }

        public abstract IDictionary<string, object> Execute(T model);

        [IgnoreDataMember]
        public bool HasUIElement
        {
            get { return View.IsNullOrWhiteSpace() == false; }
        }
        //[IgnoreDataMember]
        //public Func<T, IDictionary<string, object>> ExecuteCallback { get; set; }
    }

    [DataContract(Name = "step", Namespace = "")]
    public abstract class InstallSetupStep
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "view")]
        public abstract string View { get; }

        /// <summary>
        /// Defines what order this step needs to execute on the server side since the 
        /// steps might be shown out of order on the front-end
        /// </summary>
        [IgnoreDataMember]
        public int ServerOrder { get; set; }

        /// <summary>
        /// Defines the step model type on the server side so we can bind it
        /// </summary>
        [IgnoreDataMember]
        public abstract Type StepType { get; }

    }
}