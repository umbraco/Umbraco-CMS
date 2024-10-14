import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for Media Type items that fetches data from the server
 * @class UmbMediaTypeItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbMediaTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	MediaTypeItemResponseModel,
	UmbMediaTypeItemModel
> {
	/**
	 * Creates an instance of UmbMediaTypeItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
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
const getItems = (uniques: Array<string>) => MediaTypeService.getItemMediaType({ id: uniques });

const mapper = (item: MediaTypeItemResponseModel): UmbMediaTypeItemModel => {
	return {
		entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
		icon: item.icon || null,
		name: item.name,
		unique: item.id,
	};
};
