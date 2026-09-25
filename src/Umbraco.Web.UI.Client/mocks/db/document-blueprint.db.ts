import type { UmbMockDocumentBlueprintModel } from '../data/mock-data-set.types.js';
import { UmbMockEntityTreeManager } from './utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from './utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { UmbMockEntityFolderManager } from './utils/entity/entity-folder.manager.js';
import { umbDocumentTypeMockDb } from './document-type.db.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateDocumentBlueprintRequestModel,
	CreateFolderRequestModel,
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
	folder = new UmbMockEntityFolderManager<UmbMockDocumentBlueprintModel>(
		this,
		createMockDocumentBlueprintFolderMapper,
	);

	constructor(data: Array<UmbMockDocumentBlueprintModel>) {
		super('documentBlueprint', data);
	}
}

const treeItemMapper = (model: UmbMockDocumentBlueprintModel): DocumentBlueprintTreeItemResponseModel => {
	const documentType = model.documentType ? umbDocumentTypeMockDb.read(model.documentType.id) : undefined;
	if (model.documentType && !documentType) {
		throw new Error(`Document type with id ${model.documentType.id} not found`);
	}

	return {
		documentType: documentType ? { icon: documentType.icon, id: documentType.id } : null,
		hasChildren: model.hasChildren,
		id: model.id,
		isFolder: model.isFolder,
		name: model.name,
		parent: model.parent,
		flags: model.flags,
		noAccess: model.noAccess,
	};
};

const createMockDocumentBlueprintFolderMapper = (
	request: CreateFolderRequestModel,
): UmbMockDocumentBlueprintModel => ({
	documentType: null,
	hasChildren: false,
	id: request.id ? request.id : UmbId.new(),
	isFolder: true,
	name: request.name,
	parent: request.parent,
	values: [],
	variants: [],
	flags: [],
	noAccess: false,
});

const createMockDocumentBlueprintMapper = (
	request: CreateDocumentBlueprintRequestModel,
): UmbMockDocumentBlueprintModel => {
	const documentType = umbDocumentTypeMockDb.read(request.documentType.id);
	if (!documentType) throw new Error(`Document type with id ${request.documentType.id} not found`);

	const now = new Date().toString();

	return {
		documentType: {
			id: documentType.id,
			icon: documentType.icon,
			collection: undefined, // TODO: get list from doc type when ready
		},
		hasChildren: false,
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
		noAccess: false,
	};
};

const detailResponseMapper = (model: UmbMockDocumentBlueprintModel): DocumentBlueprintResponseModel => {
	// Folders are read through the folder endpoints, so anything reaching here is a blueprint.
	if (!model.documentType) throw new Error(`Document blueprint with id ${model.id} has no document type`);

	return {
		documentType: model.documentType,
		id: model.id,
		values: model.values,
		variants: model.variants,
		flags: model.flags,
	};
};

const itemMapper = (model: UmbMockDocumentBlueprintModel): DocumentBlueprintItemResponseModel => {
	// Folders are read through the folder endpoints, so anything reaching here is a blueprint.
	if (!model.documentType) throw new Error(`Document blueprint with id ${model.id} has no document type`);

	return {
		documentType: {
			collection: model.documentType.collection,
			icon: model.documentType.icon,
			id: model.documentType.id,
		},
		id: model.id,
		name: model.name,
		flags: model.flags,
	};
};

export const umbDocumentBlueprintMockDb = new UmbDocumentBlueprintMockDB([]);
