import { UmbControllerEvent } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbEntityActionEventArgs extends UmbEntityModel {
	eventUnique?: string;
}

export class UmbEntityActionEvent<
	ArgsType extends UmbEntityActionEventArgs = UmbEntityActionEventArgs,
> extends UmbControllerEvent {
	protected _args: ArgsType;

	public constructor(type: string, args: ArgsType) {
		super(type);
		this._args = args;
	}

	getEntityType(): string {
		return this._args.entityType;
	}

	getUnique(): string | null {
		return this._args.unique;
	}

	getEventUnique(): string | undefined {
		return this._args.eventUnique;
	}
}
