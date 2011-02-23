package org.umbraco.desktopmediauploader.events
{
	import flash.events.Event;
	
	public class SignedInEvent extends Event
	{
		public static const SIGNED_IN:String = "signedIn";
		
		public function SignedInEvent(type:String, bubbles:Boolean=false, cancelable:Boolean=false)
		{
			super(type, bubbles, cancelable);
		}

		override public function clone():Event {
			return new SignedInEvent(type, bubbles, cancelable);
		}
	}
}