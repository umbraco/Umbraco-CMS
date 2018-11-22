namespace umbraco.MacroEngines
{
	internal static class LegacyConverter
	{
		/// <summary>
		/// Checks if the object is DynamicXml or DynamicNull and ensures that we return the legacy class not the new one 
		/// as we want this class to always ensure we're dealing with the legacy classes
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		internal static object ConvertToLegacy(object result)
		{
			if (result is Umbraco.Core.Dynamics.DynamicXml)
			{
                result = new DynamicXml(((Umbraco.Core.Dynamics.DynamicXml)result).BaseElement);
			}
			else if (result is Umbraco.Core.Dynamics.DynamicNull)
			{
				result = new DynamicNull();
			}
			else if (result is Umbraco.Core.Dynamics.DynamicDictionary)
			{
				result = new DynamicDictionary(((Umbraco.Core.Dynamics.DynamicDictionary) result).SourceItems);
			}
			
			return result;
		}
	}
}