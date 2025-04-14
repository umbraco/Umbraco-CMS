import { UMB_LANGUAGE_ENTITY_TYPE } from '../../entity.js';
import type { UmbLanguageItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { LanguageService } from '@umbraco-cms/backoffice/external/backend-api';
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
	/**
	 * Creates an instance of UmbLanguageItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLanguageItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => LanguageService.getItemLanguage({ query: { isoCode: uniques } });

const mapper = (item: LanguageItemResponseModel): UmbLanguageItemModel => {
	return {
		unique: item.isoCode,
		name: item.name,
		entityType: UMB_LANGUAGE_ENTITY_TYPE,
	};
};
