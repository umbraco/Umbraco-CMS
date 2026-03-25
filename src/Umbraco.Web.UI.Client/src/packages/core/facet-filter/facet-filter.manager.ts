import { UMB_FACET_FILTER_MANAGER_CONTEXT } from './facet-filter.manager.context-token.js';
import type { UmbActiveFacetFilterModel, UmbFacetFilterValueModel } from './types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFacetedResultModel {
	alias: string;
	result: unknown;
}

export class UmbFacetFilterManager extends UmbContextBase {
	#activeFilters = new UmbArrayState<UmbActiveFacetFilterModel>([], (x) => `${x.alias}||${x.unique}`);
	public readonly activeFilters = this.#activeFilters.asObservable();
	public readonly totalActiveFilters = this.#activeFilters.asObservablePart(
		(filters) => new Set(filters.map((f) => f.alias)).size,
	);

	#facetedResults = new UmbArrayState<UmbFacetedResultModel>([], (x) => x.alias);

	constructor(host: UmbControllerHost) {
		super(host, UMB_FACET_FILTER_MANAGER_CONTEXT);
	}

	/**
	 * Add or update a single filter entry identified by alias + unique.
	 * @param filter
	 */
	public setFilter(filter: UmbActiveFacetFilterModel): void {
		const key = `${filter.alias}||${filter.unique}`;
		if (this.#activeFilters.getHasOne(key)) {
			this.#activeFilters.updateOne(key, filter);
		} else {
			this.#activeFilters.append([filter]);
		}
	}

	/**
	 * Atomically replace all entries for a given alias.
	 * @param alias
	 * @param entries
	 */
	public setFilterValues(alias: string, entries: Array<{ unique: string; value: unknown }>): void {
		const current = this.#activeFilters.getValue();
		const kept = current.filter((f) => f.alias !== alias);
		const added = entries.map((e) => ({ alias, unique: e.unique, value: e.value }));
		this.#activeFilters.setValue([...kept, ...added]);
	}

	/**
	 * Observable for all active filter entries for a given alias.
	 * @param alias
	 */
	public activeFiltersByAlias(alias: string): Observable<Array<UmbActiveFacetFilterModel>> {
		return this.#activeFilters.asObservablePart((filters) => filters.filter((f) => f.alias === alias));
	}

	/**
	 * Clear all entries for a given alias.
	 * @param alias
	 */
	public clearFilter(alias: string): void {
		const current = this.#activeFilters.getValue();
		this.#activeFilters.setValue(current.filter((f) => f.alias !== alias));
	}

	/**
	 * Clear a single active filter entry identified by alias + unique.
	 * @param alias
	 * @param unique
	 */
	public clearActiveFilter(alias: string, unique: string): void {
		const key = `${alias}||${unique}`;
		this.#activeFilters.remove([key]);
	}

	/**
	 * Clear all active filters.
	 */
	public clearAllFilters(): void {
		this.#activeFilters.setValue([]);
	}

	/**
	 * Get all currently active filter values (without runtime keys).
	 */
	public async getActiveFilterValues(): Promise<Array<UmbFacetFilterValueModel>> {
		return this.#activeFilters.getValue().map(({ alias, value }) => ({ alias, value }));
	}

	/**
	 * Set faceted result data for a given alias.
	 * @param alias
	 * @param result
	 */
	public setFacetedResult(alias: string, result: unknown): void {
		const key = alias;
		if (this.#facetedResults.getHasOne(key)) {
			this.#facetedResults.updateOne(key, { alias, result });
		} else {
			this.#facetedResults.append([{ alias, result }]);
		}
	}

	/**
	 * Observable for the faceted result of a given alias.
	 * @param alias
	 */
	public facetedResultByAlias(alias: string): Observable<unknown> {
		return this.#facetedResults.asObservablePart((results) => results.find((r) => r.alias === alias)?.result);
	}

	/**
	 * Clear all faceted results.
	 */
	public clearFacetedResults(): void {
		this.#facetedResults.setValue([]);
	}
}
