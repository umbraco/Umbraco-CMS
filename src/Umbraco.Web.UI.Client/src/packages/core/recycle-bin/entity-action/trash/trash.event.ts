import { UmbEntityActionEvent, type UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbEntityTrashedEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'entity-trashed';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbEntityTrashedEvent.TYPE, args);
	}
}
