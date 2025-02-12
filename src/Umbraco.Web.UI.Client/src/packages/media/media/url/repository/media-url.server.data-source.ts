import type { UmbMediaUrlModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService, type MediaUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A server data source for Media Urls
 * @class UmbMediaUrlServerDataSource
 * @implements {DocumentTreeDataSource}
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
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MediaService.getMediaUrls({ id: uniques });

const mapper = (item: MediaUrlInfoResponseModel): UmbMediaUrlModel => {
	const url = item.urlInfos.length ? item.urlInfos[0].url : undefined;
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
