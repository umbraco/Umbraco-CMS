import { UmbMockEntityTreeManager } from '../entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../entity/entity-detail.manager.js';
import { umbDocumentTypeMockDb } from '../document-type/document-type.db.js';
import { UmbEntityMockDbBase } from '../entity/entity-base.js';
import { UmbEntityRecycleBin } from '../entity/entity-recycle-bin.js';
import { UmbMockDocumentModel, data } from './document.data.js';
import { UmbMockDocumentPublishingManager } from './document-publishing.manager.js';
import {
	CreateDocumentRequestModel,
	DocumentItemResponseModel,
	DocumentResponseModel,
	DocumentTreeItemResponseModel,
	PagedDocumentTypeResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbDocumentMockDB extends UmbEntityMockDbBase<UmbMockDocumentModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDocumentModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockDocumentModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDocumentModel>(this, createMockDocumentMapper, detailResponseMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockDocumentModel>(this.data, treeItemMapper);
	publishing = new UmbMockDocumentPublishingManager(this);

	constructor(data: Array<UmbMockDocumentModel>) {
		super(data);
	}

	getAllowedDocumentTypes(id: string): PagedDocumentTypeResponseModel {
		debugger;
		const { contentTypeId } = this.detail.read(id);

		if (contentTypeId) {
			const docType = umbDocumentTypeMockDb.detail.read(contentTypeId);

			if (docType) {
				const allowedTypes = docType.allowedContentTypes;
				const models = allowedTypes
					.map((allowedType: any) => umbDocumentTypeMockDb.detail.read(allowedType.id))
					.filter((item: any) => item !== undefined);
				const total = models.length;
				return { items: models, total };
			}
		}

		return { items: [], total: 0 };
	}

	// permissions
	/*
	getUserPermissionsForDocument(id: string): Array<any> {
		return umbUserPermissionData
			.getAll()
			.items.filter(
				(permission: any) =>
					permission.target.entityType === UMB_DOCUMENT_ENTITY_TYPE && permission.target.documentId === id,
			);
	}
  */
}

const treeItemMapper = (model: UmbMockDocumentModel): Omit<DocumentTreeItemResponseModel, 'type'> => {
	const documentType = umbDocumentTypeMockDb.read(model.contentTypeId);
	if (!documentType) throw new Error(`Document type with id ${model.contentTypeId} not found`);

	return {
		id: model.id,
		parentId: model.parentId,
		contentTypeId: model.contentTypeId,
		variants: model.variants,
		icon: documentType.icon,
		isContainer: documentType.isContainer,
		name: model.variants?.[0]?.name,
		hasChildren: model.hasChildren,
		noAccess: model.noAccess,
		isProtected: model.isProtected,
		isPublished: model.isProtected,
		isEdited: model.isEdited,
		isTrashed: model.isTrashed,
	};
};

const createMockDocumentMapper = (request: CreateDocumentRequestModel): UmbMockDocumentModel => {
	const documentType = umbDocumentTypeMockDb.read(request.contentTypeId);
	if (!documentType) throw new Error(`Document type with id ${request.contentTypeId} not found`);

	return {
		id: request.id ? request.id : UmbId.new(),
		parentId: request.parentId,
		contentTypeId: request.contentTypeId,
		variants: request.variants,
		values: request.values,
		name: request.variants?.[0]?.name,
		icon: documentType.icon,
		isContainer: documentType.isContainer,
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isPublished: false,
		isEdited: false,
		isTrashed: false,
	};
};

const detailResponseMapper = (model: UmbMockDocumentModel): DocumentResponseModel => {
	return {
		values: model.values,
		variants: model.variants,
		id: model.id,
		contentTypeId: model.contentTypeId,
		urls: model.urls,
		templateId: model.templateId,
		isTrashed: model.isTrashed,
	};
};

const itemMapper = (model: UmbMockDocumentModel): DocumentItemResponseModel => {
	return {
		name: model.name,
		id: model.id,
		icon: model.icon,
		contentTypeId: model.contentTypeId,
		isTrashed: model.isTrashed,
	};
};

export const umbDocumentMockDb = new UmbDocumentMockDB(data);
