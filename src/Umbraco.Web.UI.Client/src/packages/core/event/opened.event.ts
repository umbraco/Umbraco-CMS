export class UmbOpenedEvent extends Event {
	public static readonly TYPE = 'opened';

	public constructor() {
		// mimics the native toggle event
		super(UmbOpenedEvent.TYPE, { bubbles: false, composed: false, cancelable: false });
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbOpenedEvent.TYPE]: UmbOpenedEvent;
	}
}
