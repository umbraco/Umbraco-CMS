import type { UmbStaticFileItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { StaticFileResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Static File items
 * @export
 * @class UmbStaticFileItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbStaticFileItemServerDataSource extends UmbItemServerDataSourceBase<
	StaticFileItemResponseModel,
	UmbStaticFileItemModel
> {
	/**
	 * Creates an instance of UmbStaticFileItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStaticFileItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => StaticFileResource.getItemStaticFile({ path: uniques });

const mapper = (item: StaticFileItemResponseModel): UmbStaticFileItemModel => {
	return {
		isFolder: item.isFolder,
		name: item.name,
		parentUnique: item.parent ? item.parent.path : null,
		unique: item.path,
	};
};
