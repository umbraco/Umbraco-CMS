export class UmbDeselectedEvent extends Event {
	public static readonly TYPE = 'deselected';
	public unique: string | null;

	public constructor(unique: string | null, args?: EventInit) {
		// mimics the native change event
		super(UmbDeselectedEvent.TYPE, { bubbles: true, composed: false, cancelable: false, ...args });
		this.unique = unique;
	}
}
