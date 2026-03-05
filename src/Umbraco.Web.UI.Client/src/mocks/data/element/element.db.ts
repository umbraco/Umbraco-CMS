import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { UmbMockEntityFolderManager } from '../utils/entity/entity-folder.manager.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbEntityRecycleBin } from '../utils/entity/entity-recycle-bin.js';
import { UmbMockElementPublishingManager } from './element-publishing.manager.js';
import { data } from './element.data.js';
import type { UmbMockElementModel } from './element.data.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateElementRequestModel,
	CreateFolderRequestModel,
	ElementConfigurationResponseModel,
	ElementItemResponseModel,
	ElementResponseModel,
	ElementTreeItemResponseModel,
	ElementValueResponseModel,
	ElementRecycleBinItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbElementMockDB extends UmbEntityMockDbBase<UmbMockElementModel> {
	tree = new UmbMockEntityTreeManager<UmbMockElementModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockElementModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockElementModel>(this, createMockElementMapper, detailResponseMapper);
	folder = new UmbMockEntityFolderManager<UmbMockElementModel>(this, createMockElementFolderMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockElementModel>(this.data, recycleBinItemMapper);
	publishing = new UmbMockElementPublishingManager(this);

	constructor(data: Array<UmbMockElementModel>) {
		super(data);
	}

	getConfiguration(): ElementConfigurationResponseModel {
		return {
			allowEditInvariantFromNonDefault: true,
			allowNonExistingSegmentsCreation: false,
			disableDeleteWhenReferenced: true,
			disableUnpublishWhenReferenced: true,
		};
	}
}

const treeItemMapper = (model: UmbMockElementModel): ElementTreeItemResponseModel => {
	return {
		hasChildren: model.hasChildren,
		id: model.id,
		parent: model.parent,
		flags: model.flags,
		name: model.name,
		isFolder: model.isFolder,
		createDate: model.createDate,
		documentType: model.documentType,
		variants: model.variants,
		noAccess: model.noAccess,
	};
};

const recycleBinItemMapper = (model: UmbMockElementModel): ElementRecycleBinItemResponseModel => {
	return {
		id: model.id,
		createDate: model.createDate,
		hasChildren: model.hasChildren,
		parent: model.parent ? { id: model.parent.id } : null,
		documentType: model.documentType,
		variants: model.variants,
		isFolder: model.isFolder,
		name: model.name,
	};
};

const createMockElementMapper = (request: CreateElementRequestModel): UmbMockElementModel => {
	const isRoot = request.parent === null || request.parent === undefined;
	let ancestors: Array<{ id: string }> = [];

	if (!isRoot) {
		const parentId = request.parent!.id;
		const parentAncestors = umbElementMockDb.tree.getAncestorsOf({ descendantId: parentId }).map((ancestor) => {
			return {
				id: ancestor.id,
			};
		});
		ancestors = [...parentAncestors, { id: parentId }];
	}

	const now = new Date().toISOString();

	return {
		ancestors,
		documentType: {
			id: request.documentType.id,
			icon: 'icon-brick',
		},
		hasChildren: false,
		id: request.id ? request.id : UmbId.new(),
		createDate: now,
		isTrashed: false,
		isFolder: false,
		name: request.variants[0]?.name || 'Untitled Element',
		parent: request.parent,
		values: request.values as ElementValueResponseModel[],
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
		flags: [],
		noAccess: false,
	};
};

const createMockElementFolderMapper = (request: CreateFolderRequestModel): UmbMockElementModel => {
	const now = new Date().toISOString();

	let ancestors: Array<{ id: string }> = [];
	if (request.parent) {
		const parentId = request.parent.id;
		const parentAncestors = umbElementMockDb.tree.getAncestorsOf({ descendantId: parentId }).map((ancestor) => {
			return {
				id: ancestor.id,
			};
		});
		ancestors = [...parentAncestors, { id: parentId }];
	}

	return {
		ancestors,
		documentType: null,
		hasChildren: false,
		id: request.id ? request.id : UmbId.new(),
		createDate: now,
		isTrashed: false,
		isFolder: true,
		name: request.name,
		parent: request.parent,
		values: [],
		variants: [],
		flags: [],
		noAccess: false,
	};
};

const detailResponseMapper = (model: UmbMockElementModel): ElementResponseModel => {
	return {
		documentType: model.documentType!,
		id: model.id,
		isTrashed: model.isTrashed,
		values: model.values,
		variants: model.variants.map((v) => ({
			culture: v.culture,
			segment: null,
			name: v.name,
			createDate: model.createDate,
			updateDate: model.createDate,
			state: v.state,
			publishDate: null,
			scheduledPublishDate: null,
			scheduledUnpublishDate: null,
		})),
		flags: model.flags,
	};
};

const itemMapper = (model: UmbMockElementModel): ElementItemResponseModel => {
	return {
		documentType: model.documentType!,
		hasChildren: model.hasChildren,
		id: model.id,
		parent: model.parent,
		variants: model.variants,
		flags: model.flags,
	};
};

export const umbElementMockDb = new UmbElementMockDB(data);
