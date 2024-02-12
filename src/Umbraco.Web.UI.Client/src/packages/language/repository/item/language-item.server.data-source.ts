import type { UmbLanguageItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { LanguageResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Language items
 * @export
 * @class UmbLanguageItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbLanguageItemServerDataSource extends UmbItemServerDataSourceBase<
	LanguageItemResponseModel,
	UmbLanguageItemModel
> {
	/**
	 * Creates an instance of UmbLanguageItemServerDataSource.
	 * @param {UmbControllerHost} host
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
const getItems = (uniques: Array<string>) => LanguageResource.getItemLanguage({ isoCode: uniques });

const mapper = (item: LanguageItemResponseModel): UmbLanguageItemModel => {
	return {
		unique: item.isoCode,
		name: item.name,
	};
};
