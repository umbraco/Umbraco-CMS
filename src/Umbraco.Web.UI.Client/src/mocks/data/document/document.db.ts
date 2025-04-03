import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { umbDocumentTypeMockDb } from '../document-type/document-type.db.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbEntityRecycleBin } from '../utils/entity/entity-recycle-bin.js';
import { data } from './document.data.js';
import { UmbMockDocumentCollectionManager } from './document-collection.manager.js';
import { UmbMockDocumentPublishingManager } from './document-publishing.manager.js';
import type { UmbMockDocumentModel } from './document.data.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
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
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentMockDB extends UmbEntityMockDbBase<UmbMockDocumentModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDocumentModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockDocumentModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDocumentModel>(this, createMockDocumentMapper, detailResponseMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockDocumentModel>(this.data, treeItemMapper);
	publishing = new UmbMockDocumentPublishingManager(this);
	collection = new UmbMockDocumentCollectionManager(this, collectionMapper);

	constructor(data: Array<UmbMockDocumentModel>) {
		super(data);
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
			reservedFieldNames: [],
		};
	}
}

const treeItemMapper = (model: UmbMockDocumentModel): DocumentTreeItemResponseModel => {
	const documentType = umbDocumentTypeMockDb.read(model.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${model.documentType.id} not found`);

	return {
		ancestorIds: model.ancestorIds,
		documentType: {
			icon: documentType.icon,
			id: documentType.id,
		},
		hasChildren: model.hasChildren,
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		noAccess: model.noAccess,
		parent: model.parent,
		variants: model.variants,
		createDate: model.createDate,
	};
};

const createMockDocumentMapper = (request: CreateDocumentRequestModel): UmbMockDocumentModel => {
	const documentType = umbDocumentTypeMockDb.read(request.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${request.documentType.id} not found`);

	const now = new Date().toString();

	return {
		ancestorIds: [],
		documentType: {
			id: documentType.id,
			icon: documentType.icon,
			collection: undefined, // TODO: get list from doc type when ready
		},
		hasChildren: false,
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
				state: DocumentVariantStateModel.DRAFT,
				publishDate: null,
			};
		}),
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
			collection: model.documentType.collection,
			icon: model.documentType.icon,
			id: model.documentType.id,
		},
		hasChildren: model.hasChildren,
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		parent: model.parent,
		variants: model.variants,
	};
};

const collectionMapper = (model: UmbMockDocumentModel): DocumentCollectionResponseModel => {
	return {
		creator: null,
		documentType: {
			id: model.documentType.id,
			alias: '',
			icon: model.documentType.icon,
		},
		id: model.id,
		isProtected: model.isProtected,
		isTrashed: model.isTrashed,
		sortOrder: 0,
		updater: null,
		values: model.values,
		variants: model.variants,
		ancestorIds: model.ancestorIds,
	};
};

export const umbDocumentMockDb = new UmbDocumentMockDB(data);
