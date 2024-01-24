import { UmbControllerEvent } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityActionEventArgs {
	unique: string;
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

	getUnique(): string {
		return this.#args.unique;
	}
}
