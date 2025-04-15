import { UmbSortChildrenOfEntityAction } from '@umbraco-cms/backoffice/tree';

/**
 * Entity action for sorting children of a content item
 * @class UmbSortChildrenOfContentEntityAction
 * @augments UmbSortChildrenOfEntityAction
 */
export class UmbSortChildrenOfContentEntityAction extends UmbSortChildrenOfEntityAction {
	override execute() {
		alert('Sort children of content action executed');
	}
}

export { UmbSortChildrenOfContentEntityAction as api };
