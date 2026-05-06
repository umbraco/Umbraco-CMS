export class UmbScrollEvent extends Event {
	public static readonly TYPE = 'umb-scroll';

	public constructor() {
		// mimics the native scroll event
		super(UmbScrollEvent.TYPE, { bubbles: true, composed: true, cancelable: false });
	}
}
