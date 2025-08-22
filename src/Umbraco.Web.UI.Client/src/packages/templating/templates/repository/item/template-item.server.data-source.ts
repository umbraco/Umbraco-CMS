import { UMB_TEMPLATE_ENTITY_TYPE } from '../../entity.js';
import type { UmbTemplateItemModel } from './types.js';
import { UmbManagementApiTemplateItemDataRequestManager } from './template-item.server.request-manager.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { TemplateItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Template items
 * @class UmbTemplateItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbTemplateItemServerDataSource extends UmbItemServerDataSourceBase<
	TemplateItemResponseModel,
	UmbTemplateItemModel
> {
	#itemRequestManager = new UmbManagementApiTemplateItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbTemplateItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTemplateItemServerDataSource
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

const mapper = (item: TemplateItemResponseModel): UmbTemplateItemModel => {
	return {
		entityType: UMB_TEMPLATE_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
		alias: item.alias,
	};
};
