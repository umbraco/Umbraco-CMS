export class UmbInputEvent extends Event {
	public static readonly TYPE = 'input';

	public constructor() {
		// mimics the native input event
		super(UmbInputEvent.TYPE, { bubbles: true, composed: true, cancelable: false });
	}
}
