import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import { UMB_COLLECTION_FACET_FILTER_CONTEXT } from './collection-facet-filter.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export class UmbCollectionFacetFilterContext extends UmbContextBase {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	#filterAlias = new UmbStringState<string | undefined>(undefined);
	public readonly filterAlias = this.#filterAlias.asObservable();

	#value: Observable<any> | undefined;
	public get value(): Observable<any> | undefined {
		return this.#value;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_COLLECTION_FACET_FILTER_CONTEXT);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			this.#observeFilterValue();
		});
	}

	public setFilterAlias(alias: string): void {
		this.#filterAlias.setValue(alias);
		this.#observeFilterValue();
	}

	public getFilterAlias(): string | undefined {
		return this.#filterAlias.getValue();
	}

	#observeFilterValue(): void {
		const alias = this.#filterAlias.getValue();
		if (!alias || !this.#collectionContext) return;

		this.#value = this.#collectionContext.filtering.filterValueByAlias(alias);
	}

	public setValue(value: unknown): void {
		const alias = this.#filterAlias.getValue();
		if (!alias || !this.#collectionContext) return;

		this.#collectionContext.filtering.setFilter({ alias, value });
		this.#collectionContext.loadCollection();
	}

	public clearValue(): void {
		const alias = this.#filterAlias.getValue();
		if (!alias || !this.#collectionContext) return;

		this.#collectionContext.filtering.clearFilter(alias);
		this.#collectionContext.loadCollection();
	}
}
