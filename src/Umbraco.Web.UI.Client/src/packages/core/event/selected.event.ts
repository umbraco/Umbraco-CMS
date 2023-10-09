export class UmbSelectedEvent extends Event {
	public constructor() {
		// mimics the native change event
		super('selected', { bubbles: true, composed: false, cancelable: false });
	}
}
