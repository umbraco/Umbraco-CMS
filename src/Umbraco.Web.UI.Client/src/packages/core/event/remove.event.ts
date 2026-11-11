export class UmbRemoveEvent extends Event {
	public static readonly TYPE = 'remove';

	public constructor() {
		super(UmbRemoveEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}

declare global {
	interface GlobalEventHandlersEventMap {
		[UmbRemoveEvent.TYPE]: UmbRemoveEvent;
	}
}
