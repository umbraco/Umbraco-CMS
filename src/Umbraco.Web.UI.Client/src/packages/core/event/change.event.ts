export class UmbChangeEvent extends Event {
	public static readonly TYPE = 'change';

	public constructor() {
		// mimics the native change event
		super(UmbChangeEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}
