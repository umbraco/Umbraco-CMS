import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../utils/entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
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
	DataTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

class UmbDataTypeMockDB extends UmbEntityMockDbBase<UmbMockDataTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDataTypeModel>(this, treeItemMapper);
	folder = new UmbMockEntityFolderManager<UmbMockDataTypeModel>(this, createFolderMockMapper);
	item = new UmbMockEntityItemManager<UmbMockDataTypeModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDataTypeModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockDataTypeModel>) {
		super(data);
	}
}

const treeItemMapper = (model: UmbMockDataTypeModel): DataTypeTreeItemResponseModel => {
	return {
		name: model.name,
		hasChildren: model.hasChildren,
		id: model.id,
		parent: model.parent,
		isFolder: model.isFolder,
		isDeletable: model.isDeletable,
	};
};

const createFolderMockMapper = (request: CreateFolderRequestModel): UmbMockDataTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		parent: request.parent,
		isFolder: true,
		hasChildren: false,
		editorAlias: '',
		editorUiAlias: '',
		isDeletable: true,
		canIgnoreStartNodes: false,
		values: [],
	};
};

const createDetailMockMapper = (request: CreateDataTypeRequestModel): UmbMockDataTypeModel => {
	return {
		id: request.id ? request.id : UmbId.new(),
		parent: request.parent,
		name: request.name,
		editorAlias: request.editorAlias,
		editorUiAlias: request.editorUiAlias,
		values: request.values,
		canIgnoreStartNodes: false,
		isFolder: false,
		hasChildren: false,
		isDeletable: true,
	};
};

const detailResponseMapper = (item: UmbMockDataTypeModel): DataTypeResponseModel => {
	return {
		id: item.id,
		name: item.name,
		editorAlias: item.editorAlias,
		editorUiAlias: item.editorUiAlias,
		values: item.values,
		isDeletable: item.isDeletable,
		canIgnoreStartNodes: item.canIgnoreStartNodes,
	};
};

const itemResponseMapper = (item: UmbMockDataTypeModel): DataTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		editorAlias: item.editorAlias,
		isDeletable: item.isDeletable,
	};
};

export const umbDataTypeMockDb = new UmbDataTypeMockDB(data);
