import {
	CreateDataTypeRequestModel,
	CreateFolderRequestModel,
	FolderTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const createTreeItem = (item: CreateDataTypeRequestModel): FolderTreeItemResponseModel => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	return {
		type: 'data-type',
		parentId: item.parentId,
		name: item.name,
		id: item.id,
		isFolder: false,
		isContainer: false,
		hasChildren: false,
	};
};

export const createFolderTreeItem = (item: CreateFolderRequestModel): FolderTreeItemResponseModel => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	return {
		...createTreeItem(item),
		isFolder: true,
	};
};
