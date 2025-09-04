import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserItemModel } from './types.js';
import { UmbManagementApiUserItemDataRequestManager } from './user-item.server.request-manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UserItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for user items that fetches data from the server
 * @class UmbUserItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbUserItemServerDataSource extends UmbItemServerDataSourceBase<UserItemResponseModel, UmbUserItemModel> {
	#itemRequestManager = new UmbManagementApiUserItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbUserItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const { data, error } = await this.#itemRequestManager.getItems(uniques);

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: UserItemResponseModel): UmbUserItemModel => {
	return {
		avatarUrls: item.avatarUrls,
		entityType: UMB_USER_ENTITY_TYPE,
		name: item.name,
		unique: item.id,
		kind: item.kind,
	};
};
