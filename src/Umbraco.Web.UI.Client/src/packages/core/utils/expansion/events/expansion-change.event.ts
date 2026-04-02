export class UmbExpansionChangeEvent extends Event {
	public static readonly TYPE = 'expansion-change';

	public constructor() {
		// mimics the native change event
		super(UmbExpansionChangeEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}
