export class UmbDeleteEvent extends Event {
	public constructor() {
		super('delete', { bubbles: true, composed: false, cancelable: false });
	}
}
