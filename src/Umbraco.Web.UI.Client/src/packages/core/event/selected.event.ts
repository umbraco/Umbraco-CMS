export class UmbSelectedEvent extends Event {
	public static readonly TYPE = 'selected';
	public unique: string;

	public constructor(unique: string) {
		// mimics the native change event
		super(UmbSelectedEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
		this.unique = unique;
	}
}
