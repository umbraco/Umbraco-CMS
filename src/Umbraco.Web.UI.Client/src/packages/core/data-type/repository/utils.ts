import { DATA_TYPE_ENTITY_TYPE } from '../entities.js';
import {
	CreateDataTypeRequestModel,
	DataTypeTreeItemResponseModel,
	FolderTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbCreateFolderModel } from '@umbraco-cms/backoffice/repository';

export const dataTypeToTreeItemMapper = (item: CreateDataTypeRequestModel): DataTypeTreeItemResponseModel => {
	if (!item) throw new Error('item is null or undefined');
	if (!item.id) throw new Error('Id is null or undefined');
	if (item.parentId === undefined) throw new Error('ParentId is undefined');

	return {
		id: item.id,
		parentId: item.parentId,
		type: DATA_TYPE_ENTITY_TYPE,
		name: item.name,
	};
};

export const folderToDataTypeTreeItemMapper = (folder: UmbCreateFolderModel): FolderTreeItemResponseModel => {
	if (!folder) throw new Error('Folder is required');
	if (!folder.unique) throw new Error('Folder unique required');
	if (folder.parentUnique === undefined) throw new Error('Folder parent unique is required');

	return {
		id: folder.unique,
		parentId: folder.parentUnique,
		type: DATA_TYPE_ENTITY_TYPE,
		name: folder.name,
		isFolder: true,
	};
};
