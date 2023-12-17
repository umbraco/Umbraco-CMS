import { UmbControllerEvent } from '@umbraco-cms/backoffice/controller-api';

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

	// TODO: this can be removed when the server supports reloading a tree item without reloading the parent
	getParentUnique(): string | null {
		return this.#args.parentUnique;
	}
}
