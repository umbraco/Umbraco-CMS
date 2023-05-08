export class UmbPropertyValueChangeEvent extends Event {
	public constructor() {
		// mimics the native change event
		super('property-value-change', { bubbles: true, composed: false, cancelable: false });
	}
}
