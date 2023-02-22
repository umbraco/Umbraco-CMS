import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { SavedLogSearchModel } from '@umbraco-cms/backend-api';

/**
 * @export
 * @class UmbLogSearchesStore
 * @extends {UmbStoreBase}
 * @description - Data Store for log searches
 */
export class UmbLogSearchesStore extends UmbStoreBase {
	#data = new ArrayState<SavedLogSearchModel>([], (x) => x.name);

	/**
	 * Creates an instance of UmbLogSearchesStore.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbLogSearchesStore
	 */
	constructor(host: UmbControllerHostInterface) {
		super(host, UmbLogSearchesStore.name);
	}

	/**
	 * Append a template to the store
	 * @param {SavedLogSearch} search
	 * @memberof UmbLogSearchesStore
	 */
	append(search: SavedLogSearchModel) {
		this.#data.append([search]);
	}

	/**
	 * Removes templates in the store with the given uniques
	 * @param {string[]} searchNames
	 * @memberof UmbLogSearchesStore
	 */
	remove(searchNames: string[]) {
		this.#data.remove(searchNames);
	}
}

export const UMB_LOG_SEARCHES_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbLogSearchesStore>(
	UmbLogSearchesStore.name,
	'LogSearchesStore TOKEN XXX'
);
