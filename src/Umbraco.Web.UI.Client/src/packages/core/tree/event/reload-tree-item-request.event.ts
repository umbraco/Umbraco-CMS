import { UmbEntityActionEvent, UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbReloadTreeItemRequestEntityActionEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'reload-tree-item-request';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbReloadTreeItemRequestEntityActionEvent.TYPE, args);
	}
}
