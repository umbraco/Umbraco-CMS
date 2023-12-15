export class UmbControllerEvent extends Event {
	public constructor(type: string) {
		super(type, { bubbles: false, composed: false, cancelable: false });
	}
}

export interface UmbActionEventArgs {
	entityType: string;
	unique: string;
}

export class UmbActionEvent extends UmbControllerEvent {
	#args: UmbActionEventArgs;

	public constructor(type: string, args: UmbActionEventArgs) {
		super(type);
		this.#args = args;
	}

	getEntityType(): string {
		return this.#args.entityType;
	}

	getUnique(): string {
		return this.#args.unique;
	}
}
