import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaRecycleBinTreeItemModel } from './types.js';
import type { MediaRecycleBinItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Media Recycle Bin tree that fetches data from the server
 * @export
 * @class UmbMediaRecycleBinTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMediaRecycleBinTreeServerDataSource extends UmbTreeServerDataSourceBase<
	MediaRecycleBinItemResponseModel,
	UmbMediaRecycleBinTreeItemModel
> {
	/**
	 * Creates an instance of UmbMediaRecycleBinTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaRecycleBinTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			getAncestorsOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	MediaResource.getRecycleBinMediaRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaResource.getRecycleBinMediaChildren({
			parentId: args.parentUnique,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	MediaResource.getTreeMediaAncestors({
		descendantId: args.descendantUnique,
	});

const mapper = (item: MediaRecycleBinItemResponseModel): UmbMediaRecycleBinTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		entityType: UMB_MEDIA_ENTITY_TYPE,
		noAccess: false,
		isTrashed: true,
		hasChildren: item.hasChildren,
		mediaType: {
			unique: item.mediaType.id,
			icon: item.mediaType.icon,
			collection: item.mediaType.collection ? { unique: item.mediaType.collection.id } : null,
		},
		variants: item.variants.map((variant) => {
			return {
				name: variant.name,
				culture: variant.culture || null,
				segment: null, // TODO: add segment to the backend API?
			};
		}),
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
		isFolder: false,
	};
};
