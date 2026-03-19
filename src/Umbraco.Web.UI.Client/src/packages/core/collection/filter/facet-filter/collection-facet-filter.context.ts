import { UMB_COLLECTION_FACET_FILTER_CONTEXT } from './collection-facet-filter.context-token.js';
import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbActiveCollectionFacetFilterModel } from './collection-facet-filter.manager.js';

export class UmbCollectionFacetFilterContext extends UmbContextBase {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	#alias = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#alias.asObservable();

	#values: Observable<Array<UmbActiveCollectionFacetFilterModel>> | undefined;
	public get values(): Observable<Array<UmbActiveCollectionFacetFilterModel>> | undefined {
		return this.#values;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_FACET_FILTER_CONTEXT);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
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
		if (!alias || !this.#collectionContext) return;

		this.#values = this.#collectionContext.filtering.filterValuesByAlias(alias);
	}

	public setValues(entries: Array<{ unique: string; value: any }>): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#collectionContext) return;

		this.#collectionContext.filtering.setFilterValues(alias, entries);
		this.#collectionContext.loadCollection();
	}

	public clearValue(unique: string): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#collectionContext) return;

		this.#collectionContext.filtering.clearFilterValue(alias, unique);
		this.#collectionContext.loadCollection();
	}

	public clearAllValues(): void {
		const alias = this.#alias.getValue();
		if (!alias || !this.#collectionContext) return;

		this.#collectionContext.filtering.clearFilter(alias);
		this.#collectionContext.loadCollection();
	}
}
