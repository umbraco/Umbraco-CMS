import type { UmbMediaUrlModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';
import { MediaService, type MediaUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';

/**
 * A server data source for Media Urls
 * @class UmbMediaUrlServerDataSource
 * @augments {UmbItemServerDataSourceBase}
 */
export class UmbMediaUrlServerDataSource extends UmbItemServerDataSourceBase<
	MediaUrlInfoResponseModel,
	UmbMediaUrlModel
> {
	/**
	 * Creates an instance of UmbMediaUrlServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaUrlServerDataSource
	 */
	#dataTypeUnique?: string;

	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});

		// A picker configured to ignore user start nodes may reference media outside them,
		// which the server only resolves when told which data type the request originates from.
		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context?.dataType,
				(dataType) => {
					this.#dataTypeUnique = dataType?.unique;
				},
				'_observeDataType',
			);
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbItemDataApiGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => MediaService.getMediaUrls({ query: { id: args.uniques, dataTypeId: this.#dataTypeUnique } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: MediaUrlInfoResponseModel): UmbMediaUrlModel => {
	const url = item.urlInfos.length ? (item.urlInfos[0].url ?? undefined) : undefined;
	const extension = url ? url.slice(url.lastIndexOf('.') + 1, url.length) : undefined;

	return {
		unique: item.id,
		url,
		extension,
		/*info: item.urlInfos.map((urlInfo) => ({
			...urlInfo,
			extension: '',
		})),*/
	};
};
