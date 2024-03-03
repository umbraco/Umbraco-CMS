import { UmbControllerEvent } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityActionEventArgs {
	unique: string | null;
	entityType: string;
}

export class UmbEntityActionEvent extends UmbControllerEvent {
	#args: UmbEntityActionEventArgs;

	public constructor(type: string, args: UmbEntityActionEventArgs) {
		super(type);
		this.#args = args;
	}

	getEntityType(): string {
		return this.#args.entityType;
	}

	getUnique(): string | null {
		return this.#args.unique;
	}
}
