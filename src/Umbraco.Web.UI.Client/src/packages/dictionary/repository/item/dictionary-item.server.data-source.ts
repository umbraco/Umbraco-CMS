import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import type { UmbDictionaryItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type {
	DictionaryItemItemResponseModel,
	DictionaryItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Dictionary items
 * @export
 * @class UmbDictionaryItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDictionaryItemServerDataSource extends UmbItemServerDataSourceBase<
	DictionaryItemItemResponseModel,
	UmbDictionaryItemModel
> {
	/**
	 * Creates an instance of UmbDictionaryItemServerDataSource.
	 * @param {UmbControllerHost} host
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
const getItems = (uniques: Array<string>) => DictionaryResource.getItemDictionary({ id: uniques });

const mapper = (item: DictionaryItemItemResponseModel): UmbDictionaryItemModel => {
	return {
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
	};
};
