import type { UmbMockDocumentModel } from '../data/mock-data-set.types.js';
import { UmbMockEntityTreeManager } from './utils/entity/entity-tree.manager.js';
import { UmbMockEntityVariantItemManager } from './utils/entity/entity-variant-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { UmbMockEntityVariantUrlManager } from './utils/entity/entity-variant-url.manager.js';
import { umbDocumentTypeMockDb } from './document-type.db.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbEntityRecycleBin } from './utils/entity/entity-recycle-bin.js';
import { UmbMockDocumentCollectionManager } from './document-collection.manager.js';
import { UmbMockDocumentPublishingManager } from './document-publishing.manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	DocumentCollectionResponseModel,
	CreateDocumentRequestModel,
	DocumentItemResponseModel,
	DocumentResponseModel,
	DocumentTreeItemResponseModel,
	DomainsResponseModel,
	DocumentConfigurationResponseModel,
	DocumentValueResponseModel,
	DocumentVariantResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export class UmbDocumentMockDB extends UmbEntityMockDbBase<UmbMockDocumentModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDocumentModel>(this, treeItemMapper);
	item = new UmbMockEntityVariantItemManager<UmbMockDocumentModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDocumentModel>(this, createMockDocumentMapper, detailResponseMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockDocumentModel>(this, treeItemMapper);
	publishing = new UmbMockDocumentPublishingManager(this);
	collection = new UmbMockDocumentCollectionManager(this, collectionMapper);
	url = new UmbMockEntityVariantUrlManager<UmbMockDocumentModel>(this);

	constructor(data: Array<UmbMockDocumentModel>) {
		super('document', data);
	}

	// permissions
	getUserPermissionsForDocument(): Array<any> {
		return [];
	}

	getDomainsForDocument(): DomainsResponseModel {
		return { defaultIsoCode: 'en-us', domains: [] };
	}

	getConfiguration(): DocumentConfigurationResponseModel {
		return {
			allowEditInvariantFromNonDefault: true,
			allowNonExistingSegmentsCreation: true,
			disableDeleteWhenReferenced: true,
			disableUnpublishWhenReferenced: true,
		};
	}
}

export const documentTypeReference = (id: string) => {
	const documentType = umbDocumentTypeMockDb.read(id);
	if (!documentType) throw new Error(`Document type with id ${id} not found`);

	return {
		icon: documentType.icon,
		id: documentType.id,
		collection: documentType.collection,
	};
};

const treeItemMapper = (model: UmbMockDocumentModel): Omit<DocumentTreeItemResponseModel, 'hasChildren'> => {
	return {
		ancestors: umbDocumentMockDb.getAncestorIds(model.id),
		documentType: documentTypeReference(model.documentType.id),
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		noAccess: model.noAccess,
		parent: model.parent,
		variants: model.variants,
		createDate: model.createDate,
		flags: model.flags,
	};
};

const createMockDocumentMapper = (request: CreateDocumentRequestModel): UmbMockDocumentModel => {
	const documentType = umbDocumentTypeMockDb.read(request.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${request.documentType.id} not found`);

	const now = new Date().toString();

	return {
		documentType: { id: documentType.id },
		id: request.id ? request.id : UmbId.new(),
		createDate: now,
		isProtected: false,
		isTrashed: false,
		noAccess: false,
		parent: request.parent,
		// TODO: Currently trusting we did send the editorAlias to the create end point:
		values: request.values as DocumentValueResponseModel[],
		variants: request.variants.map((variantRequest) => {
			return {
				culture: variantRequest.culture,
				segment: variantRequest.segment,
				name: variantRequest.name,
				createDate: now,
				updateDate: now,
				state: 'Draft' as UmbDocumentVariantState,
				publishDate: null,
				id: UmbId.new(),
				flags: [],
			};
		}),
		flags: [],
	};
};

const detailResponseMapper = (model: UmbMockDocumentModel): DocumentResponseModel => {
	return {
		documentType: documentTypeReference(model.documentType.id),
		id: model.id,
		isTrashed: model.isTrashed,
		template: model.template,
		values: model.values,
		variants: model.variants,
		flags: model.flags,
	};
};

const itemMapper = (model: UmbMockDocumentModel): DocumentItemResponseModel => {
	return {
		documentType: documentTypeReference(model.documentType.id),
		hasChildren: umbDocumentMockDb.hasChildren(model.id),
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		parent: model.parent,
		variants: model.variants,
		flags: model.flags,
	};
};

const collectionMapper = (model: UmbMockDocumentModel): DocumentCollectionResponseModel => {
	const documentType = umbDocumentTypeMockDb.read(model.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${model.documentType.id} not found`);

	return {
		ancestors: umbDocumentMockDb.getAncestorIds(model.id),
		creator: null,
		documentType: {
			id: documentType.id,
			alias: documentType.alias,
			icon: documentType.icon,
			collection: documentType.collection,
		},
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		sortOrder: 0,
		updater: null,
		values: model.values,
		variants: model.variants,
		flags: model.flags,
		hasChildren: umbDocumentMockDb.hasChildren(model.id),
	};
};

export const umbDocumentMockDb = new UmbDocumentMockDB([]);
