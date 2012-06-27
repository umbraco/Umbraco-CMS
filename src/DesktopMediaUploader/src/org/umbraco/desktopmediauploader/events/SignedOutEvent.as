package org.umbraco.desktopmediauploader.events
{
	import flash.events.Event;
	
	public class SignedOutEvent extends Event
	{
		public static const SIGNED_OUT:String = "signedOut";

		public function SignedOutEvent(type:String, bubbles:Boolean=false, cancelable:Boolean=false)
		{
			super(type, bubbles, cancelable);
		}

		override public function clone():Event {
			return new SignedOutEvent(type, bubbles, cancelable);
		}
	}
}