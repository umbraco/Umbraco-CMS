import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { umbMediaTypeMockDb } from '../media-type/media-type.db.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbEntityRecycleBin } from '../utils/entity/entity-recycle-bin.js';
import { UmbMockMediaCollectionManager } from './media-collection.manager.js';
import { data } from './media.data.js';
import type { UmbMockMediaModel } from './media.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateMediaRequestModel,
	MediaCollectionResponseModel,
	MediaItemResponseModel,
	MediaResponseModel,
	MediaTreeItemResponseModel,
	MediaValueResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMediaMockDB extends UmbEntityMockDbBase<UmbMockMediaModel> {
	tree = new UmbMockEntityTreeManager<UmbMockMediaModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockMediaModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMediaModel>(this, createMockMediaMapper, detailResponseMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockMediaModel>(this.data, treeItemMapper);
	collection = new UmbMockMediaCollectionManager(this, collectionMapper);

	constructor(data: Array<UmbMockMediaModel>) {
		super(data);
	}
}

const treeItemMapper = (model: UmbMockMediaModel): MediaTreeItemResponseModel => {
	const mediaType = umbMediaTypeMockDb.read(model.mediaType.id);
	if (!mediaType) throw new Error(`Media type with id ${model.mediaType.id} not found`);

	return {
		mediaType: {
			collection: model.mediaType.collection,
			icon: model.mediaType.icon,
			id: model.mediaType.id,
		},
		hasChildren: model.hasChildren,
		id: model.id,
		isTrashed: model.isTrashed,
		noAccess: model.noAccess,
		parent: model.parent,
		variants: model.variants,
		createDate: model.createDate,
	};
};

const createMockMediaMapper = (request: CreateMediaRequestModel): UmbMockMediaModel => {
	const mediaType = umbMediaTypeMockDb.read(request.mediaType.id);
	if (!mediaType) throw new Error(`Media type with id ${request.mediaType.id} not found`);

	const now = new Date().toString();

	return {
		mediaType: {
			id: mediaType.id,
			icon: mediaType.icon,
			collection: mediaType.collection,
		},
		hasChildren: false,
		id: request.id ? request.id : UmbId.new(),
		createDate: now,
		isTrashed: false,
		noAccess: false,
		parent: request.parent,
		// We trust blindly that we send of the editorAlias to the create end point.
		values: request.values as MediaValueResponseModel[],
		variants: request.variants.map((variantRequest) => {
			return {
				culture: variantRequest.culture,
				segment: variantRequest.segment,
				name: variantRequest.name,
				createDate: now,
				updateDate: now,
				publishDate: null,
			};
		}),
		urls: [],
	};
};

const detailResponseMapper = (model: UmbMockMediaModel): MediaResponseModel => {
	return {
		mediaType: model.mediaType,
		id: model.id,
		isTrashed: model.isTrashed,
		urls: model.urls,
		values: model.values,
		variants: model.variants,
	};
};

const itemMapper = (model: UmbMockMediaModel): MediaItemResponseModel => {
	return {
		mediaType: {
			collection: model.mediaType.collection,
			icon: model.mediaType.icon,
			id: model.mediaType.id,
		},
		hasChildren: model.hasChildren,
		id: model.id,
		isTrashed: model.isTrashed,
		parent: model.parent,
		variants: model.variants,
	};
};

const collectionMapper = (model: UmbMockMediaModel): MediaCollectionResponseModel => {
	return {
		creator: null,
		id: model.id,
		mediaType: {
			id: model.mediaType.id,
			alias: '',
			icon: model.mediaType.icon,
		},
		sortOrder: 0,
		values: model.values,
		variants: model.variants,
	};
};

export const umbMediaMockDb = new UmbMediaMockDB(data);
