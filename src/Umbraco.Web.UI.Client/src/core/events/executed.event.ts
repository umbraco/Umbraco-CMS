export class UmbExecutedEvent extends Event {
	public constructor() {
		super('executed', { bubbles: true, composed: true, cancelable: false });
	}
}
