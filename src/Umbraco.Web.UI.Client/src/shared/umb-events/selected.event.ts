export class UmbSelectionChangeEvent extends Event {
	public static readonly TYPE = 'selection-change';

	public constructor() {
		// mimics the native change event
		super('selection-change', { bubbles: true, composed: false, cancelable: false });
	}
}
