import type { UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbDataTypeTreeItemModel = {
	unique: string;
	parentUnique: string | null;
	isFolder: boolean;
	isContainer: boolean;
	name: string;
	type: string;
	hasChildren: boolean;
};

// TODO: TREE STORE TYPE PROBLEM:
export interface UmbDataTypeTreeRootModel extends UmbEntityTreeRootModel {}
