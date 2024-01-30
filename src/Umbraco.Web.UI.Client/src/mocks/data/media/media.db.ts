import { UmbMockEntityTreeManager } from '../utils/entity/entity-tree.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { umbMediaTypeMockDb } from '../media-type/media-type.db.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbEntityRecycleBin } from '../utils/entity/entity-recycle-bin.js';
import type { UmbMockMediaModel } from './media.data.js';
import { data } from './media.data.js';
import {
	ContentStateModel,
	type CreateMediaRequestModel,
	type MediaItemResponseModel,
	type MediaResponseModel,
	type MediaTreeItemResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbMediaMockDB extends UmbEntityMockDbBase<UmbMockMediaModel> {
	tree = new UmbMockEntityTreeManager<UmbMockMediaModel>(this, treeItemMapper);
	item = new UmbMockEntityItemManager<UmbMockMediaModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMediaModel>(this, createMockMediaMapper, detailResponseMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockMediaModel>(this.data, treeItemMapper);

	constructor(data: Array<UmbMockMediaModel>) {
		super(data);
	}
}

const treeItemMapper = (model: UmbMockMediaModel): Omit<MediaTreeItemResponseModel, 'type'> => {
	const mediaType = umbMediaTypeMockDb.read(model.mediaType.id);
	if (!mediaType) throw new Error(`Media type with id ${model.mediaType.id} not found`);

	return {
		mediaType: {
			hasListView: model.mediaType.hasListView,
			icon: model.mediaType.icon,
			id: model.mediaType.id,
		},
		hasChildren: model.hasChildren,
		id: model.id,
		isTrashed: model.isTrashed,
		noAccess: model.noAccess,
		parent: model.parent,
		variants: model.variants,
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
			hasListView: false, // TODO: get list from doc type when ready
		},
		hasChildren: false,
		id: request.id ? request.id : UmbId.new(),
		isTrashed: false,
		noAccess: false,
		parent: request.parent,
		values: request.values,
		variants: request.variants.map((variantRequest) => {
			return {
				culture: variantRequest.culture,
				segment: variantRequest.segment,
				name: variantRequest.name,
				createDate: now,
				updateDate: now,
				state: ContentStateModel.DRAFT,
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
			hasListView: model.mediaType.hasListView,
			icon: model.mediaType.icon,
			id: model.mediaType.id,
		},
		id: model.id,
		isTrashed: model.isTrashed,
		variants: model.variants,
	};
};

export const umbMediaMockDb = new UmbMediaMockDB(data);
