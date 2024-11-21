export class UmbStoreEvent extends Event {
	public uniques: Array<string> = [];

	public constructor(type: string, uniques: Array<string>) {
		super(type, { bubbles: false, composed: false, cancelable: false });
		this.uniques = uniques;
	}
}
