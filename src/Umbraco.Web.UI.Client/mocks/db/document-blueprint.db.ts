import type { UmbMockDocumentBlueprintModel } from '../data/mock-data-set.types.js';
import { UmbMockEntityTreeManager } from './utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from './utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { umbDocumentTypeMockDb } from './document-type.db.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateDocumentBlueprintRequestModel,
	DocumentBlueprintItemResponseModel,
	DocumentBlueprintResponseModel,
	DocumentBlueprintTreeItemResponseModel,
	DocumentValueResponseModel,
	DocumentVariantResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export class UmbDocumentBlueprintMockDB extends UmbEntityMockDbBase<UmbMockDocumentBlueprintModel> {
	tree = new UmbMockEntityTreeManager<UmbMockDocumentBlueprintModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockDocumentBlueprintModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockDocumentBlueprintModel>(
		this,
		createMockDocumentBlueprintMapper,
		detailResponseMapper,
	);

	constructor(data: Array<UmbMockDocumentBlueprintModel>) {
		super('documentBlueprint', data);
	}
}

const documentTypeReference = (id: string) => {
	const documentType = umbDocumentTypeMockDb.read(id);
	if (!documentType) throw new Error(`Document type with id ${id} not found`);

	return {
		collection: documentType.collection,
		icon: documentType.icon,
		id: documentType.id,
	};
};

const treeItemMapper = (
	model: UmbMockDocumentBlueprintModel,
): Omit<DocumentBlueprintTreeItemResponseModel, 'hasChildren'> => {
	return {
		documentType: documentTypeReference(model.documentType.id),
		id: model.id,
		isFolder: model.isFolder,
		name: model.name,
		parent: model.parent,
		flags: model.flags,
		variants: model.isFolder ? [] : model.variants,
	};
};

const createMockDocumentBlueprintMapper = (
	request: CreateDocumentBlueprintRequestModel,
): UmbMockDocumentBlueprintModel => {
	const documentType = umbDocumentTypeMockDb.read(request.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${request.documentType.id} not found`);

	const now = new Date().toString();

	return {
		documentType: { id: documentType.id },
		id: request.id ? request.id : UmbId.new(),
		isFolder: false,
		name: request.variants[0].name,
		parent: request.parent,
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

const detailResponseMapper = (model: UmbMockDocumentBlueprintModel): DocumentBlueprintResponseModel => {
	return {
		documentType: documentTypeReference(model.documentType.id),
		id: model.id,
		values: model.values,
		variants: model.variants,
		flags: model.flags,
	};
};

const itemMapper = (model: UmbMockDocumentBlueprintModel): DocumentBlueprintItemResponseModel => {
	return {
		documentType: documentTypeReference(model.documentType.id),
		id: model.id,
		name: model.name,
		flags: model.flags,
	};
};

export const umbDocumentBlueprintMockDb = new UmbDocumentBlueprintMockDB([]);
