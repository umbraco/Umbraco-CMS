import type { UmbPickerModalSearchManagerConfig } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { debounce } from '@umbraco-cms/backoffice/utils';

/**
 * A manager for searching items in a picker modal.
 * @exports
 * @class UmbPickerModalSearchManager
 * @augments {UmbControllerBase}
 * @template ResultItemType
 * @template QueryType
 */
export class UmbPickerModalSearchManager<
	ResultItemType extends UmbEntityModel = UmbEntityModel,
	QueryType extends UmbSearchRequestArgs = UmbSearchRequestArgs,
> extends UmbControllerBase {
	#query = new UmbObjectState<QueryType | undefined>(undefined);
	public readonly query = this.#query.asObservable();

	#searching = new UmbBooleanState(false);
	public readonly searching = this.#searching.asObservable();

	#resultItems = new UmbArrayState<ResultItemType>([], (x) => x.unique);
	public readonly resultItems = this.#resultItems.asObservable();

	#resultTotalItems = new UmbNumberState(0);
	public readonly resultTotalItems = this.#resultTotalItems.asObservable();

	#config?: UmbPickerModalSearchManagerConfig;
	// TODO: lower requirement for search provider item type
	#searchProvider?: UmbSearchProvider<any, QueryType>;

	/**
	 * Creates an instance of UmbPickerModalSearchManager.
	 * @param {UmbControllerHost} host The controller host for the search manager.
	 * @memberof UmbPickerModalSearchManager
	 */
	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Set the configuration for the search manager.
	 * @param {UmbPickerModalSearchManagerConfig} config The configuration for the search manager.
	 * @memberof UmbPickerModalSearchManager
	 */
	public setConfig(config: UmbPickerModalSearchManagerConfig) {
		this.#config = config;
		this.#initSearch();
	}

	/**
	 * Get the current configuration for the search manager.
	 * @returns {UmbPickerModalSearchManagerConfig | undefined} The current configuration for the search manager.
	 * @memberof UmbPickerModalSearchManager
	 */
	public getConfig(): UmbPickerModalSearchManagerConfig | undefined {
		return this.#config;
	}

	/**
	 * Search for items based on the current query.
	 * @memberof UmbPickerModalSearchManager
	 */
	public search() {
		this.#searching.setValue(true);
		this.#debouncedSearch();
	}

	/**
	 * Clear the current search query and result items.
	 * @memberof UmbPickerModalSearchManager
	 */
	public clear() {
		this.#query.setValue(undefined);
		this.#resultItems.setValue([]);
		this.#searching.setValue(false);
	}

	/**
	 * Set the search query.
	 * @param {QueryType} query The search query.
	 * @memberof UmbPickerModalSearchManager
	 */
	public setQuery(query: QueryType) {
		if (!this.query) {
			this.clear();
			return;
		}

		this.#query.setValue(query);
	}

	/**
	 * Get the current search query.
	 * @memberof UmbPickerModalSearchManager
	 * @returns {QueryType | undefined} The current search query.
	 */
	public getQuery(): QueryType | undefined {
		return this.#query.getValue();
	}

	async #initSearch() {
		const providerAlias = this.#config?.providerAlias;
		if (!providerAlias) throw new Error('No search provider alias provided');
		this.#searchProvider = await createExtensionApiByAlias<UmbSearchProvider>(this, providerAlias);
		if (!this.#searchProvider) throw new Error(`Search Provider with alias ${providerAlias} is not available`);
	}

	#debouncedSearch = debounce(this.#search, 300);

	async #search() {
		if (!this.#searchProvider) throw new Error('Search provider is not available');
		const query = this.#query.getValue();
		if (!query) throw new Error('No query provided');
		const { data } = await this.#searchProvider.search(query);
		this.#resultItems.setValue(data?.items ?? []);
		this.#resultTotalItems.setValue(data?.total ?? 0);
		this.#searching.setValue(false);
	}
}
