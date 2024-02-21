import type { UmbUserItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UserItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UserResource } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for user items that fetches data from the server
 * @export
 * @class UmbUserItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbUserItemServerDataSource extends UmbItemServerDataSourceBase<UserItemResponseModel, UmbUserItemModel> {
	/**
	 * Creates an instance of UmbUserItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => UserResource.getItemUser({ id: uniques });

const mapper = (item: UserItemResponseModel): UmbUserItemModel => {
	return {
		unique: item.id,
		name: item.name,
	};
};
