import { UMB_FACET_FILTER_CONTEXT } from './facet-filter.context-token.js';
import { UMB_FACET_FILTER_MANAGER_CONTEXT } from './facet-filter.manager.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbActiveFacetFilterModel } from './facet-filter.manager.js';

export class UmbFacetFilterContext extends UmbContextBase {
	#manager?: typeof UMB_FACET_FILTER_MANAGER_CONTEXT.TYPE;

	#alias = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#alias.asObservable();

	#values: Observable<Array<UmbActiveFacetFilterModel>> | undefined;
	public get values(): Observable<Array<UmbActiveFacetFilterModel>> | undefined {
		return this.#values;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_FACET_FILTER_CONTEXT);

		this.consumeContext(UMB_FACET_FILTER_MANAGER_CONTEXT, (manager) => {
			this.#manager = manager;
			this.#observeFilterValues();
		});
	}

	public setAlias(alias: string): void {
		this.#alias.setValue(alias);
		this.#observeFilterValues();
	}

	public getAlias(): string | undefined {
		return this.#alias.getValue();
	}

	#observeFilterValues(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#manager) return;

		this.#values = this.#manager.filterValuesByAlias(alias);
	}

	public setValues(entries: Array<{ unique: string; value: any }>): void {
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
