export class UmbSelectionChangeEvent extends Event {
	public static readonly TYPE = 'selection-change';

	public constructor(args?: EventInit) {
		// mimics the native change event
		super(UmbSelectionChangeEvent.TYPE, { bubbles: true, composed: false, cancelable: false, ...args });
	}
}
