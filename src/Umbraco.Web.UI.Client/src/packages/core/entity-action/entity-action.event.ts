import { UmbControllerEvent } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbEntityActionEventArgs extends UmbEntityModel {}

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
