package org.umbraco.desktopmediauploader.net
{
	import org.umbraco.desktopmediauploader.events.*;
	import org.umbraco.desktopmediauploader.util.*;
	
	import flash.events.*;
	import flash.net.*;
	
	public class DmuRequest extends EventDispatcher
	{
		public function DmuRequest(action:String, vars:URLVariables = null, target:IEventDispatcher = null)
		{
			super(target);
			
			var req:URLRequest = getRequest(action, vars);
			
			var loader:URLLoader = new URLLoader();
			loader.addEventListener(Event.COMPLETE, loader_Success);
			loader.addEventListener(IOErrorEvent.IO_ERROR, loader_Error);
			loader.load(req);
		}
		
		private function loader_Success(event:Event):void
		{
			var loader:URLLoader = event.target as URLLoader;
			if (loader != null)
			{
				var result:String = StringHelper.cleanXmlString(loader.data);
				var xml:XML = new XML(result);
				if (xml.@success == "true")
				{
					dispatchEvent(new DmuRequestEvent(DmuRequestEvent.SUCCESS, xml));
				}
				else
				{
					loader_Error();
				}
			}
			else
			{
				loader_Error();
			}
		}
		
		private function loader_Error(event:IOErrorEvent = null):void
		{
			dispatchEvent(new DmuRequestEvent(DmuRequestEvent.ERROR));
		}
		
		public static function getRequest(action:String, vars:URLVariables = null):URLRequest
		{
			var loaderUrl:String = Model.url +"/umbraco/webservices/mediauploader.ashx";
			
			if (vars == null)
				vars = new URLVariables();
			
			vars.action = action;
			vars.username = Model.username;
			vars.password = Model.password;
			vars.ticket = Model.ticket;
			
			var req:URLRequest = new URLRequest(loaderUrl);
			req.data = vars;
			req.method = URLRequestMethod.POST;
			
			return req;
		}
		
		public static function makeRequest(action:String, successCallback:Function, errorCallback:Function,
										   vars:URLVariables = null):void
		{
			var req:DmuRequest = new DmuRequest(action, vars);
			req.addEventListener(DmuRequestEvent.SUCCESS, successCallback);
			req.addEventListener(DmuRequestEvent.ERROR, errorCallback);
		}
	}
}