import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDataTypeTreeItemModel extends UmbUniqueTreeItemModel {
	isFolder: boolean;
	isContainer: boolean;
}

export interface UmbDataTypeTreeRootModel extends UmbUniqueTreeRootModel {}
