import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbActiveCollectionFacetFilterModel {
	alias: string;
	value: any;
}

export class UmbCollectionFacetFilterManager extends UmbControllerBase {
	#activeFilters = new UmbArrayState<UmbActiveCollectionFacetFilterModel>([], (x) => x.alias);
	public readonly activeFilters = this.#activeFilters.asObservable();
	public readonly totalActiveFilters = this.#activeFilters.asObservablePart((filters) => filters.length);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Apply a filter value. Tracks the filter as active using the manifest alias as identifier.
	 * @param {UmbActiveCollectionFacetFilterModel} filter
	 * @param filter.alias The manifest alias used as the unique identifier.
	 * @param filter.value The filter value.
	 */
	public setFilter(filter: UmbActiveCollectionFacetFilterModel): void {
		if (this.#activeFilters.getHasOne(filter.alias)) {
			this.#activeFilters.updateOne(filter.alias, filter);
		} else {
			this.#activeFilters.append([filter]);
		}
	}

	/**
	 * Observable for the active filter value by its manifest alias.
	 * @param {string} alias The manifest alias of the filter to observe.
	 * @returns {Observable<UmbActiveCollectionFacetFilterModel | undefined>} The active filter model, or undefined if not active.
	 */
	public filterValueByAlias(alias: string): Observable<UmbActiveCollectionFacetFilterModel | undefined> {
		return this.#activeFilters.asObservablePart((filters) => filters.find((f) => f.alias === alias));
	}

	/**
	 * Clear an active filter by its manifest alias.
	 * @param {string} alias The manifest alias of the filter to clear.
	 */
	public clearFilter(alias: string): void {
		this.#activeFilters.remove([alias]);
	}

	/**
	 * Clear all active filters.
	 */
	public clearAllFilters(): void {
		this.#activeFilters.setValue([]);
	}

	/**
	 * Get all currently active filters.
	 * @returns {Promise<Array<UmbActiveCollectionFacetFilterModel>>}
	 */
	public async getActiveFilters(): Promise<Array<UmbActiveCollectionFacetFilterModel>> {
		return this.#activeFilters.getValue();
	}
}
