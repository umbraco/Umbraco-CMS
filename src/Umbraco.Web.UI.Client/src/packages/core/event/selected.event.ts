export class UmbSelectedEvent extends Event {
	public static readonly TYPE = 'selected';
	public unique: string | null;

	public constructor(unique: string | null, args?: EventInit) {
		// mimics the native change event
		super(UmbSelectedEvent.TYPE, { bubbles: true, composed: false, cancelable: false, ...args });
		this.unique = unique;
	}
}
