export class UmbSortedEvent extends Event {
	public static readonly TYPE = 'sorted';

	public constructor() {
		// mimics the native change event
		super(UmbSortedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}
