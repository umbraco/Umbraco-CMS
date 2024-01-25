import { UmbEntityMockDbBase } from '../entity/entity-base.js';
import { UmbMockEntityFolderManager } from '../entity/entity-folder.manager.js';
import { UmbMockEntityTreeManager } from '../entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../entity/entity-detail.manager.js';
import type { UmbMockDocumentTypeModel } from './document-type.data.js';
import { data } from './document-type.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateDocumentTypeRequestModel,
	CreateFolderRequestModel,
	DocumentTypeItemResponseModel,
	DocumentTypeResponseModel,
	DocumentTypeTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

class UmbDocumentTypeMockDB extends UmbEntityMockDbBase<UmbMockDocumentTypeModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDocumentTypeModel>(this, documentTypeTreeItemMapper);
	folder = new UmbMockEntityFolderManager<UmbMockDocumentTypeModel>(this, createMockDocumentTypeFolderMapper);
	item = new UmbMockEntityItemManager<UmbMockDocumentTypeModel>(this, documentTypeItemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDocumentTypeModel>(
		this,
		createMockDocumentTypeMapper,
		documentTypeDetailMapper,
	);

	constructor(data: Array<UmbMockDocumentTypeModel>) {
		super(data);
	}
}

const createMockDocumentTypeFolderMapper = (request: CreateFolderRequestModel): UmbMockDocumentTypeModel => {
	return {
		name: request.name,
		id: request.id ? request.id : UmbId.new(),
		parent: request.parentId ? { id: request.parentId } : null,
		description: '',
		alias: '',
		icon: '',
		properties: [],
		containers: [],
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		allowedDocumentTypes: [],
		compositions: [],
		isFolder: true,
		hasChildren: false,
		allowedTemplates: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
	};
};

const createMockDocumentTypeMapper = (request: CreateDocumentTypeRequestModel): UmbMockDocumentTypeModel => {
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
		allowedDocumentTypes: request.allowedDocumentTypes,
		compositions: request.compositions,
		parent: request.folder,
		isFolder: false,
		hasChildren: false,
		allowedTemplates: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
	};
};

const documentTypeDetailMapper = (item: UmbMockDocumentTypeModel): DocumentTypeResponseModel => {
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
		allowedDocumentTypes: item.allowedDocumentTypes,
		compositions: item.compositions,
		allowedTemplates: item.allowedTemplates,
		cleanup: item.cleanup,
	};
};

const documentTypeTreeItemMapper = (
	item: UmbMockDocumentTypeModel,
): Omit<DocumentTypeTreeItemResponseModel, 'type'> => {
	return {
		name: item.name,
		hasChildren: item.hasChildren,
		id: item.id,
		parent: item.parent,
		isFolder: item.isFolder,
		icon: item.icon,
		isElement: item.isElement,
	};
};

const documentTypeItemMapper = (item: UmbMockDocumentTypeModel): DocumentTypeItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		icon: item.icon,
		isElement: item.isElement,
	};
};

export const umbDocumentTypeMockDb = new UmbDocumentTypeMockDB(data);
