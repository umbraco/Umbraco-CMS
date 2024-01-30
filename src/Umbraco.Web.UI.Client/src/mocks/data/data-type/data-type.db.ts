import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../utils/entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { folderTreeItemMapper } from '../utils.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockDataTypeModel } from './data-type.data.js';
import { data } from './data-type.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
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
