import { UmbEntityActionEvent, type UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbRequestReloadStructureForEntityEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'request-reload-structure-for-entity';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbRequestReloadStructureForEntityEvent.TYPE, args);
	}
}
