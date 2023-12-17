export class UmbControllerEvent extends Event {
	public constructor(type: string) {
		super(type, { bubbles: false, composed: false, cancelable: false });
	}
}

export interface UmbActionEventArgs {
	unique: string;
	parentUnique: string | null; // TODO: remove this when we have endpoints to support mapping a new item without reloading the parent tree item
}

export class UmbActionEvent extends UmbControllerEvent {
	#args: UmbActionEventArgs;

	public constructor(type: string, args: UmbActionEventArgs) {
		super(type);
		this.#args = args;
	}

	getUnique(): string {
		return this.#args.unique;
	}

	getParentUnique(): string | null {
		return this.#args.parentUnique;
	}
}
