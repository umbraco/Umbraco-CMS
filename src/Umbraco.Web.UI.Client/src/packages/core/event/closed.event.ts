export class UmbClosedEvent extends Event {
	public static readonly TYPE = 'closed';

	public constructor() {
		// mimics the native toggle event
		super(UmbClosedEvent.TYPE, { bubbles: false, composed: false, cancelable: false });
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbClosedEvent.TYPE]: UmbClosedEvent;
	}
}
