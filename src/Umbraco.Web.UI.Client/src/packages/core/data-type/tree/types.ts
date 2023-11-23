import { type DataTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type UmbDataTypeTreeItemModel = {
	unique: string;
	parentUnique: string | null;
	isFolder: boolean;
	isContainer: boolean;
	name: string;
	entityType: string;
	hasChildren: boolean;
};

export type UmbDataTypeTreeRootModel = DataTypeTreeItemResponseModel & UmbEntityTreeRootModel;
