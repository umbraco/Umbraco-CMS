import { UMB_TEMPLATE_ENTITY_TYPE } from '../../entity.js';
import type { UmbTemplateItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { TemplateItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { TemplateService } from '@umbraco-cms/backoffice/external/backend-api';
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
	/**
	 * Creates an instance of UmbTemplateItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
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
const getItems = (uniques: Array<string>) => TemplateService.getItemTemplate({ id: uniques });

const mapper = (item: TemplateItemResponseModel): UmbTemplateItemModel => {
	return {
		entityType: UMB_TEMPLATE_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
		alias: item.alias,
	};
};
