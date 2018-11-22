using System;
using System.ComponentModel;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.Slider
{
    /// <summary>
    /// The options for the Slider data-type.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SliderOptions : AbstractOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SliderOptions"/> class.
        /// </summary>
        public SliderOptions()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderOptions"/> class.
        /// </summary>
        /// <param name="loadDefaults">if set to <c>true</c> [loads defaults].</param>
        public SliderOptions(bool loadDefaults)
            : base(loadDefaults)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable range].
        /// </summary>
        /// <value><c>true</c> if [enable range]; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool EnableRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable step].
        /// </summary>
        /// <value><c>true</c> if [enable step]; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        public bool EnableStep { get; set; }

        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        /// <value>The max value.</value>
        [DefaultValue(100)]
        public double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>The min value.</value>
        [DefaultValue(0)]
        public double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        /// <value>The orientation.</value>
        [DefaultValue("hortizontal")]
        public string Orientation { get; set; }

        /// <summary>
        /// Gets or sets the range value.
        /// </summary>
        /// <value>The range value.</value>
        [DefaultValue("")]
        public string RangeValue { get; set; }

        /// <summary>
        /// Gets or sets the step.
        /// </summary>
        /// <value>The step.</value>
        [DefaultValue(5)]
        public double StepValue { get; set; }
        //public int StepValue { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [DefaultValue(50)]
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the second value.
        /// </summary>
        /// <value>The second value.</value>
        [DefaultValue(0)]
        public double Value2 { get; set; }

        /// <summary>
        /// Gets or sets the Database Storage Type 
        /// </summary>
        [DefaultValue(DBTypes.Integer)]
        public DBTypes DBType { get; set; }
    }
}
