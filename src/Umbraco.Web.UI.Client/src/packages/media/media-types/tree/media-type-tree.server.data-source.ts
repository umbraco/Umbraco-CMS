import { UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaTypeTreeItemModel } from './types.js';
import type { MediaTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaTypeResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeChildrenOfRequestArgs, UmbTreeRootItemsRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Media Type tree that fetches data from the server
 * @export
 * @class UmbMediaTypeTreeServerDataSource
 * @extends {UmbTreeServerDataSourceBase}
 */
export class UmbMediaTypeTreeServerDataSource extends UmbTreeServerDataSourceBase<
	MediaTypeTreeItemResponseModel,
	UmbMediaTypeTreeItemModel
> {
	/**
	 * Creates an instance of UmbMediaTypeTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeTreeServerDataSource
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
	MediaTypeResource.getTreeMediaTypeRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	if (args.parentUnique === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeResource.getTreeMediaTypeChildren({
			parentId: args.parentUnique,
		});
	}
};

const mapper = (item: MediaTypeTreeItemResponseModel): UmbMediaTypeTreeItemModel => {
	return {
		unique: item.id,
		parentUnique: item.parent ? item.parent.id : null,
		name: item.name,
		entityType: item.isFolder ? UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE : UMB_MEDIA_TYPE_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isFolder: item.isFolder,
		icon: item.icon,
	};
};
