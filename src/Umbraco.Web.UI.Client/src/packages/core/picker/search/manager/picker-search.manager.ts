import type { UmbPickerSearchManagerConfig } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { debounce, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSearchProvider, UmbSearchRequestArgs, UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A manager for searching items in a picker.
 * @exports
 * @class UmbPickerSearchManager
 * @augments {UmbControllerBase}
 * @template ResultItemType
 * @template SearchRequestArgsType
 */
export class UmbPickerSearchManager<
	ResultItemType extends UmbSearchResultItemModel = UmbSearchResultItemModel,
	SearchRequestArgsType extends UmbSearchRequestArgs = UmbSearchRequestArgs,
> extends UmbControllerBase {
	#searchable = new UmbBooleanState(false);
	public readonly searchable = this.#searchable.asObservable();

	#query = new UmbObjectState<SearchRequestArgsType | undefined>(undefined);
	public readonly query = this.#query.asObservable();

	#executedQuery = new UmbObjectState<SearchRequestArgsType | undefined>(undefined);
	public readonly executedQuery = this.#executedQuery.asObservable();

	#searching = new UmbBooleanState(false);
	public readonly searching = this.#searching.asObservable();

	#resultItems = new UmbArrayState<ResultItemType>([], (x) => x.unique);
	public readonly resultItems = this.#resultItems.asObservable();

	#resultTotalItems = new UmbNumberState(0);
	public readonly resultTotalItems = this.#resultTotalItems.asObservable();

	#pagination = new UmbPaginationManager();
	public readonly pagination = this.#pagination;

	#config?: UmbPickerSearchManagerConfig;
	#searchProvider?: UmbSearchProvider<UmbSearchResultItemModel, SearchRequestArgsType>;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#pagination.setPageSize(100);
	}

	/**
	 * Set the configuration for the search manager.
	 * @param {UmbPickerSearchManagerConfig} config The configuration for the search manager.
	 * @memberof UmbPickerSearchManager
	 */
	public setConfig(config: UmbPickerSearchManagerConfig) {
		this.#config = config;
		this.#initSearch();
	}

	/**
	 * Get the current configuration for the search manager.
	 * @returns {UmbPickerSearchManagerConfig | undefined} The current configuration for the search manager.
	 * @memberof UmbPickerSearchManager
	 */
	public getConfig(): UmbPickerSearchManagerConfig | undefined {
		return this.#config;
	}

	/**
	 * Update the current configuration for the search manager.
	 * @param {Partial<UmbPickerSearchManagerConfig>} partialConfig
	 * @memberof UmbPickerSearchManager
	 */
	public updateConfig(partialConfig: Partial<UmbPickerSearchManagerConfig>) {
		const mergedConfig = { ...this.#config, ...partialConfig } as UmbPickerSearchManagerConfig;
		this.setConfig(mergedConfig);
	}

	/**
	 * Returns whether items can be searched.
	 * @returns {boolean} Whether items can be searched.
	 * @memberof UmbPickerSearchManager
	 */
	public getSearchable(): boolean {
		return this.#searchable.getValue();
	}

	/**
	 * Sets whether items can be searched.
	 * @param {boolean} value Whether items can be searched.
	 * @memberof UmbPickerSearchManager
	 */
	public setSearchable(value: boolean) {
		this.#searchable.setValue(value);
	}

	/**
	 * Search for items based on the current query.
	 * @memberof UmbPickerSearchManager
	 */
	public search() {
		if (this.getSearchable() === false) throw new Error('Search is not enabled');

		const query = this.#query.getValue();
		if (!query?.query) {
			this.clear();
			return;
		}

		this.#searching.setValue(true);
		this.#debouncedSearch();
	}

	/**
	 * Clear the current search query and result items.
	 * @memberof UmbPickerSearchManager
	 */
	public clear() {
		this.#query.setValue(undefined);
		this.#executedQuery.setValue(undefined);
		this.#resultItems.setValue([]);
		this.#searching.setValue(false);
		this.#resultTotalItems.setValue(0);
		this.#pagination.clear();
	}

	/**
	 * Set the search query.
	 * @param {SearchRequestArgsType} query The search query.
	 * @memberof UmbPickerSearchManager
	 */
	public setQuery(query: SearchRequestArgsType) {
		if (!this.query) {
			this.clear();
			return;
		}

		this.#query.setValue(query);
	}

	/**
	 * Get the current search query.
	 * @memberof UmbPickerSearchManager
	 * @returns {SearchRequestArgsType | undefined} The current search query.
	 */
	public getQuery(): SearchRequestArgsType | undefined {
		return this.#query.getValue();
	}

	/**
	 * Update the current search query.
	 * @param {Partial<SearchRequestArgsType>} query
	 * @memberof UmbPickerSearchManager
	 */
	public updateQuery(query: Partial<SearchRequestArgsType>) {
		this.#pagination.setCurrentPageNumber(1);
		const mergedQuery = { ...this.getQuery(), ...query } as SearchRequestArgsType;
		this.#query.setValue(mergedQuery);
	}

	async #initSearch() {
		const providerAlias = this.#config?.providerAlias;
		if (!providerAlias) {
			this.setSearchable(false);
			throw new Error('No search provider alias provided');
		}

		this.#searchProvider = await createExtensionApiByAlias<UmbSearchProvider>(this, providerAlias);

		if (!this.#searchProvider) {
			this.setSearchable(false);
			throw new Error(`Search Provider with alias ${providerAlias} is not available`);
		}

		this.setSearchable(true);
	}

	#debouncedSearch = debounce(this.#search, 300);

	async #search() {
		if (this.getSearchable() === false) throw new Error('Search is not enabled');
		if (!this.#searchProvider) throw new Error('Search provider is not available');

		const query = this.#query.getValue();
		if (!query?.query) {
			this.clear();
			return;
		}

		const args = {
			...query,
			// ensure that config params are always included
			...this.#config?.queryParams,
			searchFrom: this.#config?.searchFrom,
			// TODO: Move this implementation to another place. The generic picker search manager shouldn't be aware of data types.
			dataTypeUnique: this.#config?.dataTypeUnique,
			paging: {
				skip: this.#pagination.getSkip(),
				take: this.#pagination.getPageSize(),
			},
		};

		const { data } = await this.#searchProvider.search(args);
		const items = (data?.items as ResultItemType[]) ?? [];
		this.#resultItems.setValue(items);
		const total = data?.total ?? 0;
		this.#resultTotalItems.setValue(total);
		this.#pagination.setTotalItems(total);
		this.#executedQuery.setValue(query);
		this.#searching.setValue(false);
	}
}
