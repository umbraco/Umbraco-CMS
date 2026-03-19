import { UMB_COLLECTION_FACET_FILTER_CONTEXT } from '../collection-facet-filter.context-token.js';
import type { ManifestCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import type { MetaCollectionFacetFilterSelect } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDatalistDataSource,
	UmbDatalistItemModel,
	UmbDatalistOptionModel,
} from '@umbraco-cms/backoffice/datalist-data-source';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

const ObserveValueItems = Symbol();

type UmbSelectValue = Pick<UmbDatalistOptionModel, 'unique' | 'entityType'>;

export class UmbSelectCollectionFacetFilterApi extends UmbControllerBase {
	#options = new UmbArrayState<UmbDatalistOptionModel>([], (x) => x.unique);
	public readonly options = this.#options.asObservable();

	#value = new UmbArrayState<UmbSelectValue>([], (x) => x.unique);
	public readonly value = this.#value.asObservable();

	#valueItems = new UmbArrayState<UmbDatalistItemModel>([], (x) => x.unique);
	public readonly valueItems = this.#valueItems.asObservable();

	#facetFilterContext?: typeof UMB_COLLECTION_FACET_FILTER_CONTEXT.TYPE;
	#datalistDataSource?: UmbDatalistDataSource;
	public readonly pagination = new UmbPaginationManager();

	#manifest?: ManifestCollectionFacetFilter | undefined;
	public get manifest(): ManifestCollectionFacetFilter | undefined {
		return this.#manifest;
	}
	public set manifest(manifest: ManifestCollectionFacetFilter | undefined) {
		this.#manifest = manifest;
		const meta = manifest?.meta as MetaCollectionFacetFilterSelect | undefined;
		if (meta?.datalistDataSource) {
			this.#datalistDataSource = new meta.datalistDataSource(this);
		}
	}

	constructor(host: UmbControllerHost) {
		super(host);
		this.pagination.setPageSize(100);

		this.consumeContext(UMB_COLLECTION_FACET_FILTER_CONTEXT, (context) => {
			this.#facetFilterContext = context;
			this.#observeFilterValue();
		});
	}

	#observeFilterValue() {
		if (!this.#facetFilterContext) return;
		this.observe(
			this.#facetFilterContext.value,
			(activeFilter) => {
				const values: Array<UmbSelectValue> = activeFilter?.value ?? [];
				this.#value.setValue(values);
				this.#requestValueItems(values.map((v) => v.unique));
			},
			'umbFilterValueObserver',
		);
	}

	public loadOptions() {
		this.#requestOptions();
	}

	public loadMoreOptions() {
		const nextPage = this.pagination.getCurrentPageNumber() + 1;
		if (nextPage <= this.pagination.getTotalPages()) {
			this.pagination.setCurrentPageNumber(nextPage);
			this.#requestOptions();
		}
	}

	public setValue(values: Array<UmbSelectValue>) {
		const filtered = values.filter((v) => v.unique !== '');

		if (filtered.length === 0) {
			this.#facetFilterContext?.clearValue();
		} else {
			this.#facetFilterContext?.setValue(filtered);
		}
	}

	async #requestValueItems(uniques: Array<string>) {
		if (uniques.length === 0) {
			this.#valueItems.setValue([]);
			return;
		}

		const { data, asObservable } = await this.#datalistDataSource!.requestItems(uniques);

		if (asObservable) {
			this.observe(asObservable(), (items) => this.#valueItems.setValue(items ?? []), ObserveValueItems);
		} else if (data) {
			this.#valueItems.setValue(data);
		}
	}

	async #requestOptions() {
		const { data } = await this.#datalistDataSource!.requestOptions({
			skip: this.pagination.getSkip(),
			take: this.pagination.getPageSize(),
		});

		if (data) {
			const currentOptions = this.#options.getValue();
			this.#options.setValue([...currentOptions, ...data.items]);
			this.pagination.setTotalItems(data.total);
		} else {
			if (this.pagination.getCurrentPageNumber() === 1) {
				this.pagination.clear();
				this.#options.setValue([]);
			}
		}
	}
}

export { UmbSelectCollectionFacetFilterApi as api };
