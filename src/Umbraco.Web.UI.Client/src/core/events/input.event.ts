export class UmbInputEvent extends Event {
	public constructor() {
		// mimics the native input event
		super('input', { bubbles: true, composed: true, cancelable: false });
	}
}
