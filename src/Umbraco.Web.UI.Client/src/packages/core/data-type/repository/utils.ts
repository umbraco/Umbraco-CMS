import { CreateFolderRequestModel } from '@umbraco-cms/backoffice/backend-api';

export const createFolderTreeItem = (item: CreateFolderRequestModel) => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	return {
		unique: item.id!,
		parentUnique: item.parentId!,
		name: item.name!,
		type: 'data-type-folder',
		isFolder: true,
		isContainer: false,
		hasChildren: false,
	};
};
