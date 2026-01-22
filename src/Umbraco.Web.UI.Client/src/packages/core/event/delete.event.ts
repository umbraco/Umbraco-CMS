export class UmbDeleteEvent extends Event {
	public static readonly TYPE = 'delete';

	public constructor() {
		super(UmbDeleteEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbDeleteEvent.TYPE]: UmbDeleteEvent;
	}
}
