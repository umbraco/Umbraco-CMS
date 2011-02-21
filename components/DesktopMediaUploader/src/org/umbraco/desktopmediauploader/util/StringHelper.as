package org.umbraco.desktopmediauploader.util
{
	import mx.utils.*;
	
	public class StringHelper
	{
		public static function cleanXmlString(str:String):String
		{
			// Remove any Byte Order Markers
			str = str.replace(String.fromCharCode(65279), "");
			
			// Trim whitespace
			return StringUtil.trim(str);
		}
	}
}