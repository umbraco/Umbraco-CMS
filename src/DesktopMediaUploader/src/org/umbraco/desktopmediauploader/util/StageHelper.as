package org.umbraco.desktopmediauploader.util
{
	import flash.display.Stage;
	import flash.events.Event;

	public class StageHelper
	{
		private static var _stage:Stage;
		private static var _resizeListenerSet:Boolean = false;
		
		[Bindable]
		public static var stageWidth:int = 0;
		
		[Bindable]
		public static var stageHeight:int = 0;
		
		public function StageHelper()
		{ }
		
		public static function set stage(value:Stage):void
		{
			_stage = value;
			
			addResizeListener();
			
			stage_resize();
		}
		
		private static function addResizeListener():void
		{
			if (_resizeListenerSet)
				return;
			
			if(!_stage)
				return;
			
			_stage.addEventListener(Event.RESIZE, stage_resize);
			_resizeListenerSet = true;
		}
		
		private static function stage_resize(e:Event = null):void
		{
			stageWidth = _stage.stageWidth;
			stageHeight = _stage.stageHeight;
		}
	}
}