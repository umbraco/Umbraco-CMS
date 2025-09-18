export class UmbInteractionMemoriesChangeEvent extends Event {
	public static readonly TYPE = 'interaction-memories-change';

	public constructor() {
		// mimics the native change event
		super(UmbInteractionMemoriesChangeEvent.TYPE, { bubbles: true, composed: false, cancelable: false });
	}
}
