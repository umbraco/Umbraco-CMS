import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

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
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => MediaTypeService.getItemMediaType({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: MediaTypeItemResponseModel): UmbMediaTypeItemModel => {
	return {
		entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
		icon: item.icon || null,
		name: item.name,
		unique: item.id,
	};
};
