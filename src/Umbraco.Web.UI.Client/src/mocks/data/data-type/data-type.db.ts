import { UmbEntityMockDbBase } from '../entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../entity/entity-tree.manager.js';
import { folderTreeItemMapper } from '../utils.js';
import { UmbMockEntityItemManager } from '../entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../entity/entity-detail.manager.js';
import { UmbMockDataTypeModel, data } from './data-type.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import {
	CreateDataTypeRequestModel,
	CreateFolderRequestModel,
	DataTypeItemResponseModel,
	DataTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbDataTypeMockDB extends UmbEntityMockDbBase<UmbMockDataTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDataTypeModel>(this, folderTreeItemMapper);
	folder = new UmbMockEntityFolderManager<UmbMockDataTypeModel>(this, createMockDataTypeFolderMapper);
	item = new UmbMockEntityItemManager<UmbMockDataTypeModel>(this, dataTypeItemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDataTypeModel>(this, createMockDataTypeMapper, dataTypeDetailMapper);

	constructor(data: Array<UmbMockDataTypeModel>) {
		super(data);
	}
}

const createMockDataTypeFolderMapper = (request: CreateFolderRequestModel): UmbMockDataTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		parentId: request.parentId,
		isFolder: true,
		hasChildren: false,
		editorAlias: '',
		values: [],
	};
};

const createMockDataTypeMapper = (request: CreateDataTypeRequestModel): UmbMockDataTypeModel => {
	return {
		id: request.id ? request.id : UmbId.new(),
		parentId: request.parentId,
		name: request.name,
		editorAlias: request.editorAlias,
		editorUiAlias: request.editorUiAlias,
		values: request.values,
		isFolder: false,
		hasChildren: false,
	};
};

const dataTypeDetailMapper = (item: UmbMockDataTypeModel): DataTypeResponseModel => {
	return {
		id: item.id,
		parentId: item.parentId,
		name: item.name,
		editorAlias: item.editorAlias,
		editorUiAlias: item.editorUiAlias,
		values: item.values,
	};
};

const dataTypeItemMapper = (item: UmbMockDataTypeModel): DataTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
	};
};

export const umbDataTypeMockDb = new UmbDataTypeMockDB(data);
