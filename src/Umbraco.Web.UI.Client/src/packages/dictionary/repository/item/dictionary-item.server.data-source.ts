import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import type { UmbDictionaryItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DictionaryItemItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Dictionary items
 * @class UmbDictionaryItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDictionaryItemServerDataSource extends UmbItemServerDataSourceBase<
	DictionaryItemItemResponseModel,
	UmbDictionaryItemModel
> {
	/**
	 * Creates an instance of UmbDictionaryItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDictionaryItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => DictionaryService.getItemDictionary({ id: uniques });

const mapper = (item: DictionaryItemItemResponseModel): UmbDictionaryItemModel => {
	return {
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
	};
};
