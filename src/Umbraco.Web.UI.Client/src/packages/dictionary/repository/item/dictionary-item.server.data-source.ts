import { UMB_DICTIONARY_ENTITY_TYPE } from '../../entity.js';
import type { UmbDictionaryItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DictionaryItemItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DictionaryService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemDataApiGetRequestController } from '@umbraco-cms/backoffice/entity-item';

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
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const itemRequestManager = new UmbItemDataApiGetRequestController(this, {
			// eslint-disable-next-line local-rules/no-direct-api-import
			api: (args) => DictionaryService.getItemDictionary({ query: { id: args.uniques } }),
			uniques,
		});

		const { data, error } = await itemRequestManager.request();

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: DictionaryItemItemResponseModel): UmbDictionaryItemModel => {
	return {
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
		unique: item.id,
		name: item.name,
	};
};
