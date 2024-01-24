import { UmbEntityMockDbBase } from '../entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../entity/entity-detail.manager.js';
import type { UmbMockMediaTypeModel} from './media-type.data.js';
import { data } from './media-type.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateFolderRequestModel,
	CreateMediaTypeRequestModel,
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	MediaTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbMediaTypeMockDB extends UmbEntityMockDbBase<UmbMockMediaTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockMediaTypeModel>(this, mediaTypeTreeItemMapper);
	folder = new UmbMockEntityFolderManager<UmbMockMediaTypeModel>(this, createMockMediaTypeFolderMapper);
	item = new UmbMockEntityItemManager<UmbMockMediaTypeModel>(this, mediaTypeItemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMediaTypeModel>(
		this,
		createMockMediaTypeMapper,
		mediaTypeDetailMapper,
	);

	constructor(data: Array<UmbMockMediaTypeModel>) {
		super(data);
	}
}

const createMockMediaTypeFolderMapper = (request: CreateFolderRequestModel): UmbMockMediaTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		parentId: request.parentId,
		description: '',
		alias: '',
		icon: '',
		properties: [],
		containers: [],
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		allowedContentTypes: [],
		compositions: [],
		isFolder: true,
		hasChildren: false,
		isContainer: false,
	};
};

const createMockMediaTypeMapper = (request: CreateMediaTypeRequestModel): UmbMockMediaTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		description: request.description,
		alias: request.alias,
		icon: request.icon,
		properties: request.properties,
		containers: request.containers,
		allowedAsRoot: request.allowedAsRoot,
		variesByCulture: request.variesByCulture,
		variesBySegment: request.variesBySegment,
		isElement: request.isElement,
		allowedContentTypes: request.allowedContentTypes,
		compositions: request.compositions,
		parentId: request.containerId,
		isFolder: false,
		hasChildren: false,
		isContainer: false,
	};
};

const mediaTypeDetailMapper = (item: UmbMockMediaTypeModel): MediaTypeResponseModel => {
	return {
		name: item.name,
		id: item.id,
		description: item.description,
		alias: item.alias,
		icon: item.icon,
		properties: item.properties,
		containers: item.containers,
		allowedAsRoot: item.allowedAsRoot,
		variesByCulture: item.variesByCulture,
		variesBySegment: item.variesBySegment,
		isElement: item.isElement,
		allowedContentTypes: item.allowedContentTypes,
		compositions: item.compositions,
	};
};

const mediaTypeTreeItemMapper = (item: UmbMockMediaTypeModel): Omit<MediaTypeTreeItemResponseModel, 'type'> => {
	return {
		name: item.name,
		hasChildren: item.hasChildren,
		id: item.id,
		isContainer: item.isContainer,
		parentId: item.parentId,
		isFolder: item.isFolder,
		icon: item.icon,
	};
};

const mediaTypeItemMapper = (item: UmbMockMediaTypeModel): MediaTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		icon: item.icon,
	};
};

export const umbMediaTypeMockDb = new UmbMediaTypeMockDB(data);
