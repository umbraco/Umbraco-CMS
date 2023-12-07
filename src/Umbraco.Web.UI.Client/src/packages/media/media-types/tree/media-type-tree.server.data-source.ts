import { UmbMediaTypeTreeItemModel } from './types.js';
import { MediaTypeResource, MediaTypeTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
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
			getChildrenOf,
			mapper,
		});
	}
}

const getChildrenOf = (parentUnique: string | null) => {
	if (parentUnique === null) {
		return MediaTypeResource.getTreeMediaTypeRoot({});
	} else {
		return MediaTypeResource.getTreeMediaTypeChildren({
			parentId: parentUnique,
		});
	}
};

const mapper = (item: MediaTypeTreeItemResponseModel): UmbMediaTypeTreeItemModel => {
	return {
		id: item.id!,
		parentId: item.parentId!,
		name: item.name!,
		type: 'media-type',
		hasChildren: item.hasChildren!,
		isContainer: item.isContainer!,
	};
};
