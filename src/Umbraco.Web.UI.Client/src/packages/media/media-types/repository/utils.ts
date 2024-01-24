import type { CreateFolderRequestModel } from '@umbraco-cms/backoffice/backend-api';

export const createFolderTreeItem = (item: CreateFolderRequestModel) => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('item.id is null or undefined');

	//TODO: change to Unique
	return {
		id: item.id!,
		parentId: item.parentId!,
		name: item.name!,
		entityType: 'media-type-folder',
		isFolder: true,
		isContainer: false,
		hasChildren: false,
	};
};
