import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTreeItemModel extends Omit<UmbEntityTreeItemModel, 'name'> {
	noAccess: boolean;
	isTrashed: boolean;
}

export interface UmbMediaTreeRootModel extends UmbEntityTreeRootModel {}
