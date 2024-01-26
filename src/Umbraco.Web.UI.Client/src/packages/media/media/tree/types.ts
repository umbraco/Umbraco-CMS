import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTreeItemModel extends Omit<UmbEntityTreeItemModel, 'name'> {
	noAccess: boolean;
	isTrashed: boolean;
}

// TODO: TREE STORE TYPE PROBLEM:
export interface UmbMediaTreeRootModel extends UmbEntityTreeRootModel {}
