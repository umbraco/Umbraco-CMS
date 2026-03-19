import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbActiveCollectionFacetFilterModel {
	alias: string;
	unique: string;
	value: any;
}

export class UmbCollectionFacetFilterManager extends UmbControllerBase {
	#activeFilters = new UmbArrayState<UmbActiveCollectionFacetFilterModel>(
		[],
		(x) => `${x.alias}||${x.unique}`,
	);
	public readonly activeFilters = this.#activeFilters.asObservable();
	public readonly totalActiveFilters = this.#activeFilters.asObservablePart(
		(filters) => new Set(filters.map((f) => f.alias)).size,
	);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Add or update a single filter entry identified by alias + unique.
	 */
	public setFilter(filter: UmbActiveCollectionFacetFilterModel): void {
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
	public filterValuesByAlias(alias: string): Observable<Array<UmbActiveCollectionFacetFilterModel>> {
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
	public async getActiveFilters(): Promise<Array<UmbActiveCollectionFacetFilterModel>> {
		return this.#activeFilters.getValue();
	}
}
