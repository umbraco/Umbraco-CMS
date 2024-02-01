import type { UmbTemplateItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { TemplateItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Template items
 * @export
 * @class UmbTemplateItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbTemplateItemServerDataSource extends UmbItemServerDataSourceBase<
	TemplateItemResponseModel,
	UmbTemplateItemModel
> {
	/**
	 * Creates an instance of UmbTemplateItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbTemplateItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => TemplateResource.getItemTemplate({ id: uniques });

const mapper = (item: TemplateItemResponseModel): UmbTemplateItemModel => {
	return {
		unique: item.id,
		name: item.name,
		alias: item.alias,
	};
};
