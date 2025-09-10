export class UmbInteractionMemoryChangeEvent extends Event {
	public static readonly TYPE = 'interaction-memory-change';

	public constructor() {
		// mimics the native change event
		super(UmbInteractionMemoryChangeEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}
