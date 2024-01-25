import { UmbMockEntityTreeManager } from '../entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../entity/entity-detail.manager.js';
import { umbDocumentTypeMockDb } from '../document-type/document-type.db.js';
import { UmbEntityMockDbBase } from '../entity/entity-base.js';
import { UmbEntityRecycleBin } from '../entity/entity-recycle-bin.js';
import type { UmbMockDocumentModel } from './document.data.js';
import { data } from './document.data.js';
import { UmbMockDocumentPublishingManager } from './document-publishing.manager.js';
import type {
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
	const documentType = umbDocumentTypeMockDb.read(model.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${model.documentType.id} not found`);

	return {
		documentType: {
			hasListView: model.documentType.hasListView,
			icon: model.documentType.icon,
			id: model.documentType.id,
		},
		hasChildren: model.hasChildren,
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		noAccess: model.noAccess,
		parent: model.parent,
		variants: model.variants,
	};
};

const createMockDocumentMapper = (request: CreateDocumentRequestModel): UmbMockDocumentModel => {
	const documentType = umbDocumentTypeMockDb.read(request.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${request.documentType.id} not found`);

	return {
		documentType: {
			id: documentType.id,
			icon: documentType.icon,
			hasListView: documentType.hasListView,
		},
		hasChildren: false,
		id: request.id ? request.id : UmbId.new(),
		isProtected: false,
		isTrashed: false,
		noAccess: false,
		parent: request.parent,
		values: request.values,
		variants: request.variants,
		urls: [],
	};
};

const detailResponseMapper = (model: UmbMockDocumentModel): DocumentResponseModel => {
	return {
		documentType: model.documentType,
		id: model.id,
		isTrashed: model.isTrashed,
		template: model.template,
		urls: model.urls,
		values: model.values,
		variants: model.variants,
	};
};

const itemMapper = (model: UmbMockDocumentModel): DocumentItemResponseModel => {
	return {
		documentType: {
			hasListView: model.documentType.hasListView,
			icon: model.documentType.icon,
			id: model.documentType.id,
		},
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		variants: model.variants,
	};
};

export const umbDocumentMockDb = new UmbDocumentMockDB(data);
