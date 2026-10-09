import type { UmbMockMediaModel } from '../data/mock-data-set.types.js';
import { UmbMockEntityTreeManager } from './utils/entity/entity-tree.manager.js';
import { UmbMockEntityVariantItemManager } from './utils/entity/entity-variant-item.manager.js';
import { UmbMockEntityDetailManager } from './utils/entity/entity-detail.manager.js';
import { UmbMockEntityVariantUrlManager } from './utils/entity/entity-variant-url.manager.js';
import { umbMediaTypeMockDb } from './media-type.db.js';
import { UmbEntityMockDbBase } from './utils/entity/entity-base.js';
import { UmbEntityRecycleBin } from './utils/entity/entity-recycle-bin.js';
import { UmbMockMediaCollectionManager } from './media-collection.manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateMediaRequestModel,
	GetMediaUrlsResponse,
	MediaCollectionResponseModel,
	MediaItemResponseModel,
	MediaResponseModel,
	MediaTreeItemResponseModel,
	MediaValueResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export function getMediaFileUrl(item: UmbMockMediaModel): string | null {
	const fileValue = item.values.find((v) => v.alias === 'umbracoFile');
	if (!fileValue?.value) return null;

	// ImageCropper stores { src, focalPoint, crops }, UploadField stores the path directly
	if (typeof fileValue.value === 'object' && 'src' in (fileValue.value as Record<string, unknown>)) {
		return (fileValue.value as { src: string }).src;
	}
	if (typeof fileValue.value === 'string') {
		return fileValue.value;
	}
	return null;
}

export class UmbMediaMockDB extends UmbEntityMockDbBase<UmbMockMediaModel> {
	tree = new UmbMockEntityTreeManager<UmbMockMediaModel>(this, treeItemMapper);
	item = new UmbMockEntityVariantItemManager<UmbMockMediaModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockMediaModel>(this, createMockMediaMapper, detailResponseMapper);
	recycleBin = new UmbEntityRecycleBin<UmbMockMediaModel>(this, treeItemMapper);
	collection = new UmbMockMediaCollectionManager(this, collectionMapper);
	url = new UmbMockEntityVariantUrlManager<UmbMockMediaModel>(this);

	constructor(data: Array<UmbMockMediaModel>) {
		super('media', data);
	}

	getFileUrls(ids: string[]): GetMediaUrlsResponse {
		return ids.map((id) => {
			const item = this.read(id);
			const fileUrl = item ? getMediaFileUrl(item) : null;
			return {
				id,
				urlInfos: fileUrl ? [{ culture: null, url: fileUrl }] : [],
			};
		});
	}
}

const mediaTypeReference = (id: string) => {
	const mediaType = umbMediaTypeMockDb.read(id);
	if (!mediaType) throw new Error(`Media type with id ${id} not found`);

	return {
		collection: mediaType.collection,
		icon: mediaType.icon,
		id: mediaType.id,
	};
};

const treeItemMapper = (model: UmbMockMediaModel): Omit<MediaTreeItemResponseModel, 'hasChildren'> => {
	return {
		mediaType: mediaTypeReference(model.mediaType.id),
		id: model.id,
		isTrashed: model.isTrashed,
		noAccess: model.noAccess,
		parent: model.parent,
		extension: model.extension,
		variants: model.variants,
		createDate: model.createDate,
		flags: model.flags,
	};
};

const createMockMediaMapper = (request: CreateMediaRequestModel): UmbMockMediaModel => {
	const mediaType = umbMediaTypeMockDb.read(request.mediaType.id);
	if (!mediaType) throw new Error(`Media type with id ${request.mediaType.id} not found`);

	const now = new Date().toString();

	return {
		mediaType: { id: mediaType.id },
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
		flags: [],
	};
};

const detailResponseMapper = (model: UmbMockMediaModel): MediaResponseModel => {
	return {
		mediaType: mediaTypeReference(model.mediaType.id),
		id: model.id,
		isTrashed: model.isTrashed,
		values: model.values,
		variants: model.variants,
		flags: model.flags,
	};
};

const itemMapper = (model: UmbMockMediaModel): MediaItemResponseModel => {
	return {
		mediaType: mediaTypeReference(model.mediaType.id),
		hasChildren: umbMediaMockDb.hasChildren(model.id),
		id: model.id,
		isTrashed: model.isTrashed,
		parent: model.parent,
		extension: model.extension,
		variants: model.variants,
		flags: model.flags,
	};
};

const collectionMapper = (model: UmbMockMediaModel): MediaCollectionResponseModel => {
	const mediaType = umbMediaTypeMockDb.read(model.mediaType.id);
	if (!mediaType) throw new Error(`Media type with id ${model.mediaType.id} not found`);

	return {
		creator: null,
		id: model.id,
		mediaType: {
			id: mediaType.id,
			alias: mediaType.alias,
			icon: mediaType.icon,
		},
		sortOrder: 0,
		extension: model.extension,
		values: model.values,
		variants: model.variants,
		flags: model.flags,
		hasChildren: umbMediaMockDb.hasChildren(model.id),
	};
};

export const umbMediaMockDb = new UmbMediaMockDB([]);
