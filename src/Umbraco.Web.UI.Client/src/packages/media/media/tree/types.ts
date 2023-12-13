import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTreeItemModel extends UmbEntityTreeItemModel {
	noAccess: boolean;
	isTrashed: boolean;
}

// TODO: TREE STORE TYPE PROBLEM:
export interface UmbMediaTreeRootModel extends UmbEntityTreeRootModel {}
