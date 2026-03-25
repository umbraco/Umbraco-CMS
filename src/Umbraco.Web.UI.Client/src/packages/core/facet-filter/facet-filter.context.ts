import { UMB_FACET_FILTER_CONTEXT } from './facet-filter.context-token.js';
import { UMB_FACET_FILTER_MANAGER_CONTEXT } from './facet-filter.manager.context-token.js';
import type { UmbActiveFacetFilterModel } from './types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbFacetFilterContext extends UmbContextBase {
	#manager?: typeof UMB_FACET_FILTER_MANAGER_CONTEXT.TYPE;

	#alias = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#alias.asObservable();

	#values: Observable<Array<UmbActiveFacetFilterModel>> | undefined;
	public get values(): Observable<Array<UmbActiveFacetFilterModel>> | undefined {
		return this.#values;
	}

	#facetedResult: Observable<unknown> | undefined;
	public get facetedResult(): Observable<unknown> | undefined {
		return this.#facetedResult;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_FACET_FILTER_CONTEXT);

		this.consumeContext(UMB_FACET_FILTER_MANAGER_CONTEXT, (manager) => {
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

		this.#values = this.#manager.filterValuesByAlias(alias);
	}

	#observeFacetedResult(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#facetedResult = this.#manager.facetedResultByAlias(alias);
	}

	public setValues(entries: Array<{ unique: string; value: unknown }>): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#manager.setFilterValues(alias, entries);
	}

	public clearValue(unique: string): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#manager.clearFilterValue(alias, unique);
	}

	public clearAllValues(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#manager.clearFilter(alias);
	}
}
