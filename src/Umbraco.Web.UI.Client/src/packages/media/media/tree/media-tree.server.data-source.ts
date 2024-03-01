import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaTreeItemModel } from './types.js';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { MediaResource, type MediaTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Media tree that fetches data from the server
 * @export
 * @class UmbMediaTreeServerDataSource
 * @extends {UmbTreeServerDataSourceBase}
 */
export class UmbMediaTreeServerDataSource extends UmbTreeServerDataSourceBase<
	MediaTreeItemResponseModel,
	UmbMediaTreeItemModel
> {
	/**
	 * Creates an instance of UmbMediaTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	MediaResource.getTreeMediaRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaResource.getTreeMediaChildren({
			parentId: args.parentUnique,
		});
	}
};

const mapper = (item: MediaTreeItemResponseModel): UmbMediaTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		entityType: UMB_MEDIA_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		noAccess: item.noAccess,
		isTrashed: item.isTrashed,
		isFolder: false,
		mediaType: {
			unique: item.mediaType.id,
			icon: item.mediaType.icon,
			collection: item.mediaType.collection ? { unique: item.mediaType.collection.id } : null,
		},
		name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
		variants: item.variants.map((variant) => {
			return {
				name: variant.name,
				culture: variant.culture || null,
			};
		}),
	};
};
