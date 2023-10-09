export class UmbExecutedEvent extends Event {
	public constructor() {
		super('action-executed', { bubbles: true, composed: true, cancelable: false });
	}
}
