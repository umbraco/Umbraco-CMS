import { UmbEntityActionEvent, type UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbReloadTreeItemChildrenRequestEntityActionEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'reload-tree-item-children-request';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbReloadTreeItemChildrenRequestEntityActionEvent.TYPE, args);
	}
}
