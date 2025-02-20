export class UmbControllerEvent extends Event {
	public constructor(type: string) {
		super(type, { bubbles: false, composed: false, cancelable: false });
	}
}
