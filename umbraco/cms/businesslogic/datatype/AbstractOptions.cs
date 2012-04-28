using System.ComponentModel;

namespace umbraco.cms.businesslogic.datatype
{
	/// <summary>
	/// Abstract class for the Prevalue Editor options.
	/// </summary>
	public abstract class AbstractOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractOptions"/> class.
		/// </summary>
		public AbstractOptions()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractOptions"/> class.
		/// </summary>
		/// <param name="loadDefaults">if set to <c>true</c> [load defaults].</param>
		public AbstractOptions(bool loadDefaults)
			: this()
		{
			if (loadDefaults)
			{
				// get the type of the object.
				var type = this.GetType();

				// iterate through the properties.
				foreach (var property in type.GetProperties())
				{
					// iterate through the DefaultValue attributes.
					foreach (DefaultValueAttribute attribute in property.GetCustomAttributes(typeof(DefaultValueAttribute), true))
					{
						// set the default value of the property.
						property.SetValue(this, attribute.Value, null);
					}
				}
			}
		}
	}
}