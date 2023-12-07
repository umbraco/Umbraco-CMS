import type { UmbEntityTreeRootModel, UmbUniqueTreeItemModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDataTypeTreeItemModel extends UmbUniqueTreeItemModel {
	isFolder: boolean;
	isContainer: boolean;
}

// TODO: TREE STORE TYPE PROBLEM:
export interface UmbDataTypeTreeRootModel extends UmbEntityTreeRootModel {}
