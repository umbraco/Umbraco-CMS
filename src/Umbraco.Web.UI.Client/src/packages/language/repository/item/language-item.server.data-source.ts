import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import type { UmbLanguageItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { LanguageService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDataApiItemGetRequestController } from '@umbraco-cms/backoffice/entity-item';

/**
 * A server data source for Language items
 * @class UmbLanguageItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbLanguageItemServerDataSource extends UmbItemServerDataSourceBase<
	LanguageItemResponseModel,
	UmbLanguageItemModel
> {
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

		const itemRequestManager = new UmbDataApiItemGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => LanguageService.getItemLanguage({ query: { isoCode: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

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
