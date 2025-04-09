import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { umbDocumentTypeMockDb } from '../document-type/document-type.db.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { data } from './document-blueprint.data.js';
import type { UmbMockDocumentBlueprintModel } from './document-blueprint.data.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateDocumentRequestModel,
	DocumentItemResponseModel,
	DocumentResponseModel,
	DocumentTreeItemResponseModel,
	DocumentValueResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentBlueprintMockDB extends UmbEntityMockDbBase<UmbMockDocumentBlueprintModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDocumentBlueprintModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockDocumentBlueprintModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDocumentBlueprintModel>(
		this,
		createMockDocumentBlueprintMapper,
		detailResponseMapper,
	);

	constructor(data: Array<UmbMockDocumentBlueprintModel>) {
		super(data);
	}
}

const treeItemMapper = (model: UmbMockDocumentBlueprintModel): Omit<DocumentTreeItemResponseModel, 'type'> => {
	const documentType = umbDocumentTypeMockDb.read(model.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${model.documentType.id} not found`);

	return {
		ancestors: model.ancestors,
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

const createMockDocumentBlueprintMapper = (request: CreateDocumentRequestModel): UmbMockDocumentBlueprintModel => {
	const documentType = umbDocumentTypeMockDb.read(request.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${request.documentType.id} not found`);

	const now = new Date().toString();

	return {
		ancestors: [],
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

const detailResponseMapper = (model: UmbMockDocumentBlueprintModel): DocumentResponseModel => {
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

const itemMapper = (model: UmbMockDocumentBlueprintModel): DocumentItemResponseModel => {
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

export const umbDocumentBlueprintMockDb = new UmbDocumentBlueprintMockDB(data);
