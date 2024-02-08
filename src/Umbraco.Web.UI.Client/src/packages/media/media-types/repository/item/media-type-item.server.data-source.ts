import type { UmbMediaTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { MediaTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for Media Type items that fetches data from the server
 * @export
 * @class UmbMediaTypeItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbMediaTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	MediaTypeItemResponseModel,
	UmbMediaTypeItemModel
> {
	/**
	 * Creates an instance of UmbMediaTypeItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MediaTypeResource.getItemMediaType({ id: uniques });

const mapper = (item: MediaTypeItemResponseModel): UmbMediaTypeItemModel => {
	return {
		icon: item.icon || null,
		name: item.name,
		unique: item.name,
	};
};
