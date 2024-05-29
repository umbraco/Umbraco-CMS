import { UmbEntityActionEvent, type UmbEntityActionEventArgs } from '@umbraco-cms/backoffice/entity-action';

/**
 * @deprecated Use `UmbRequestReloadChildrenOfEntityEvent` instead.
 */
export class UmbRequestReloadTreeItemChildrenEvent extends UmbEntityActionEvent {
	static readonly TYPE = 'request-reload-tree-item-children';

	constructor(args: UmbEntityActionEventArgs) {
		super(UmbRequestReloadTreeItemChildrenEvent.TYPE, args);
	}
}
