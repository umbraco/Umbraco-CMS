export class UmbDeselectedEvent extends Event {
	public static readonly TYPE = 'deselected';
	public unique: string;

	public constructor(unique: string) {
		// mimics the native change event
		super(UmbDeselectedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.unique = unique;
	}
}
