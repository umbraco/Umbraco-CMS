import { queryFilter } from '../utils.js';
import type { UmbMockDataTypeModel } from '../data/sets/index.js';
import { dataSet } from '../data/sets/index.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbMockEntityFolderManager } from './utils/entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from './utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from './utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateDataTypeRequestModel,
	CreateFolderRequestModel,
	DataTypeItemResponseModel,
	DataTypeResponseModel,
	DataTypeTreeItemResponseModel,
	PagedDataTypeItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDataTypeFilterOptions {
	skip: number;
	take: number;
	orderBy: string;
	orderDirection: string;
	editorUiAlias?: string;
	filter?: string;
}

const editorUiAliasFilter = (filterOptions: UmbDataTypeFilterOptions, item: UmbMockDataTypeModel) =>
	item.editorUiAlias === filterOptions.editorUiAlias;

const dataQueryFilter = (filterOptions: UmbDataTypeFilterOptions, item: UmbMockDataTypeModel) =>
	queryFilter(filterOptions.filter ?? '', item.name);

class UmbDataTypeMockDB extends UmbEntityMockDbBase<UmbMockDataTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDataTypeModel>(this, treeItemMapper);
	folder = new UmbMockEntityFolderManager<UmbMockDataTypeModel>(this, createFolderMockMapper);
	item = new UmbMockEntityItemManager<UmbMockDataTypeModel>(this, itemResponseMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDataTypeModel>(this, createDetailMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockDataTypeModel>) {
		super(data);
	}

	filter(options: UmbDataTypeFilterOptions): PagedDataTypeItemResponseModel {
		const allItems = this.getAll();

		const filterOptions: UmbDataTypeFilterOptions = {
			skip: options.skip || 0,
			take: options.take || 25,
			orderBy: options.orderBy || 'name',
			orderDirection: options.orderDirection || 'asc',
			editorUiAlias: options.editorUiAlias,
			filter: options.filter,
		};

		const filteredItems = allItems.filter(
			(item) => editorUiAliasFilter(filterOptions, item) && dataQueryFilter(filterOptions, item),
		);
		const totalItems = filteredItems.length;

		const paginatedItems = filteredItems.slice(filterOptions.skip, filterOptions.skip + filterOptions.take);

		return { total: totalItems, items: paginatedItems };
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
		flags: model.flags,
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
		flags: [],
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
		flags: [],
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
		flags: item.flags,
	};
};

export const umbDataTypeMockDb = new UmbDataTypeMockDB(dataSet.dataType ?? []);
