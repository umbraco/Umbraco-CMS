import { UMB_FILTER_CONTEXT } from './filter.context-token.js';
import { UMB_FILTER_MANAGER_CONTEXT } from './filter-manager.context-token.js';
import type { UmbActiveFilterModel } from './types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbFilterContext extends UmbContextBase {
	#manager?: typeof UMB_FILTER_MANAGER_CONTEXT.TYPE;

	#alias = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#alias.asObservable();

	#values = new UmbArrayState<UmbActiveFilterModel>([], (value) => value.unique);
	public readonly values = this.#values.asObservable();

	#facetedResult = new UmbBasicState<unknown>(undefined);
	public readonly facetedResult = this.#facetedResult.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_FILTER_CONTEXT);

		this.consumeContext(UMB_FILTER_MANAGER_CONTEXT, (manager) => {
			this.#manager = manager;
			this.#observeFilterValues();
			this.#observeFacetedResult();
		});
	}

	public setAlias(alias: string): void {
		this.#alias.setValue(alias);
		this.#observeFilterValues();
		this.#observeFacetedResult();
	}

	public getAlias(): string | undefined {
		return this.#alias.getValue();
	}

	#observeFilterValues(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.observe(
			this.#manager.activeFiltersByAlias(alias),
			(values) => this.#values.setValue(values),
			'umbFilterValuesObserver',
		);
	}

	#observeFacetedResult(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.observe(
			this.#manager.facetedResultByAlias(alias),
			(result) => this.#facetedResult.setValue(result),
			'umbFilterFacetedResultObserver',
		);
	}

	public setValues(entries: Array<{ unique: string; value: unknown }>): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#manager.setFilterValues(alias, entries);
	}

	public clearValue(unique: string): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#manager.clearActiveFilter(alias, unique);
	}

	public clearAllValues(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#manager.clearFilter(alias);
	}
}
