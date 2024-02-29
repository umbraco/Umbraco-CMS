export class UmbDeselectedEvent extends Event {
	public static readonly TYPE = 'deselected';
	public unique: string | null;

	public constructor(unique: string | null) {
		// mimics the native change event
		super(UmbDeselectedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.unique = unique;
	}
}
