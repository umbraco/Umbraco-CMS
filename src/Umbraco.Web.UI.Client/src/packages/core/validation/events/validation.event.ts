export class UmbValidationEvent extends Event {
	public constructor(type: string) {
		super(type, { bubbles: true, composed: false, cancelable: false });
	}
}
