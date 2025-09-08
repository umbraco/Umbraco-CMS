import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import type { UmbLanguageItemModel } from './types.js';
import { UmbManagementApiLanguageItemDataRequestManager } from './language-item.server.request-manager.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Language items
 * @class UmbLanguageItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbLanguageItemServerDataSource extends UmbItemServerDataSourceBase<
	LanguageItemResponseModel,
	UmbLanguageItemModel
> {
	#itemRequestManager = new UmbManagementApiLanguageItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbLanguageItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLanguageItemServerDataSource
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

const mapper = (item: LanguageItemResponseModel): UmbLanguageItemModel => {
	return {
		unique: item.isoCode,
		name: item.name,
		entityType: UMB_LANGUAGE_ENTITY_TYPE,
	};
};
