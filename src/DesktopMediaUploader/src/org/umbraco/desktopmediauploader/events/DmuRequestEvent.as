package org.umbraco.desktopmediauploader.events
{
	import flash.events.Event;
	
	public class DmuRequestEvent extends Event
	{
		public static const SUCCESS:String = "success";
		public static const ERROR:String = "error";
		
		public var result:XML;
		
		public function DmuRequestEvent(type:String, result:XML = null, bubbles:Boolean=false, cancelable:Boolean=false)
		{
			super(type, bubbles, cancelable);
			
			this.result = result;
		}
	}
}