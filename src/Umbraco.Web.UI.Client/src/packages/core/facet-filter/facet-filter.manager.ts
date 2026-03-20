import { UMB_FACET_FILTER_MANAGER_CONTEXT } from './facet-filter.manager.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbActiveFacetFilterModel {
	alias: string;
	unique: string;
	value: any;
}

export class UmbFacetFilterManager extends UmbContextBase {
	#activeFilters = new UmbArrayState<UmbActiveFacetFilterModel>(
		[],
		(x) => `${x.alias}||${x.unique}`,
	);
	public readonly activeFilters = this.#activeFilters.asObservable();
	public readonly totalActiveFilters = this.#activeFilters.asObservablePart(
		(filters) => new Set(filters.map((f) => f.alias)).size,
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_FACET_FILTER_MANAGER_CONTEXT);
	}

	/**
	 * Add or update a single filter entry identified by alias + unique.
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
	 */
	public setFilterValues(alias: string, entries: Array<{ unique: string; value: any }>): void {
		const current = this.#activeFilters.getValue();
		const kept = current.filter((f) => f.alias !== alias);
		const added = entries.map((e) => ({ alias, unique: e.unique, value: e.value }));
		this.#activeFilters.setValue([...kept, ...added]);
	}

	/**
	 * Observable for all active filter entries for a given alias.
	 */
	public filterValuesByAlias(alias: string): Observable<Array<UmbActiveFacetFilterModel>> {
		return this.#activeFilters.asObservablePart((filters) => filters.filter((f) => f.alias === alias));
	}

	/**
	 * Clear all entries for a given alias.
	 */
	public clearFilter(alias: string): void {
		const current = this.#activeFilters.getValue();
		this.#activeFilters.setValue(current.filter((f) => f.alias !== alias));
	}

	/**
	 * Clear a single entry identified by alias + unique.
	 */
	public clearFilterValue(alias: string, unique: string): void {
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
	 * Get all currently active filters.
	 */
	public async getActiveFilters(): Promise<Array<UmbActiveFacetFilterModel>> {
		return this.#activeFilters.getValue();
	}
}
