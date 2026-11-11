import { UMB_FACET_FILTER_CONTEXT } from '../facet-filter.context-token.js';
import type { ManifestFacetFilter } from '../facet-filter.extension.js';
import type { UmbFacetModel } from '../types.js';
import type { MetaFacetFilterSelect } from './types.js';
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

export class UmbSelectFacetFilterApi extends UmbControllerBase {
	#options = new UmbArrayState<UmbDatalistOptionModel>([], (x) => x.unique);
	public readonly options = this.#options.asObservable();

	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#valueItems = new UmbArrayState<UmbDatalistItemModel>([], (x) => x.unique);
	public readonly valueItems = this.#valueItems.asObservable();

	#facetFilterContext?: typeof UMB_FACET_FILTER_CONTEXT.TYPE;
	#datalistDataSource?: UmbDatalistDataSource;
	#hasFacetedResults = false;
	#facetOptionsRequest = 0;
	public readonly pagination = new UmbPaginationManager();

	#manifest?: ManifestFacetFilter | undefined;
	public get manifest(): ManifestFacetFilter | undefined {
		return this.#manifest;
	}
	public set manifest(manifest: ManifestFacetFilter | undefined) {
		this.#manifest = manifest;
		const meta = manifest?.meta as MetaFacetFilterSelect | undefined;
		if (meta?.datalistDataSource) {
			this.#datalistDataSource = new meta.datalistDataSource(this);
		}
	}

	constructor(host: UmbControllerHost) {
		super(host);
		this.pagination.setPageSize(100);

		this.consumeContext(UMB_FACET_FILTER_CONTEXT, (context) => {
			this.#facetFilterContext = context;
			this.#observeFilterValues();
			this.#observeFacetedResult();
		});
	}

	#observeFilterValues() {
		if (!this.#facetFilterContext) return;
		this.observe(
			this.#facetFilterContext.values,
			(entries) => {
				const values: Array<string> = entries?.map((e) => e.value as string) ?? [];
				this.#value.setValue(values);
				this.#requestValueItems(values);
			},
			'umbFilterValuesObserver',
		);
	}

	#observeFacetedResult() {
		if (!this.#facetFilterContext) return;
		this.observe(
			this.#facetFilterContext.facetedResult,
			(result) => {
				const facets = result as Array<UmbFacetModel> | undefined;
				if (facets && facets.length > 0) {
					this.#hasFacetedResults = true;
					this.#setFacetOptions(facets);
				} else {
					this.#hasFacetedResults = false;
				}
			},
			'umbFacetedResultObserver',
		);
	}

	public loadOptions() {
		if (this.#hasFacetedResults) return;
		this.#requestOptions();
	}

	public loadMoreOptions() {
		if (this.#hasFacetedResults) return;
		const nextPage = this.pagination.getCurrentPageNumber() + 1;
		if (nextPage <= this.pagination.getTotalPages()) {
			this.pagination.setCurrentPageNumber(nextPage);
			this.#requestOptions();
		}
	}

	public setValue(values: Array<string>) {
		const filtered = values.filter((v) => v !== '');

		if (filtered.length === 0) {
			this.#facetFilterContext?.clearAllValues();
		} else {
			this.#facetFilterContext?.setValues(filtered.map((v) => ({ unique: v, value: v })));
		}
	}

	async #setFacetOptions(facets: Array<UmbFacetModel>) {
		const request = ++this.#facetOptionsRequest;
		const uniques = facets.map((facet) => facet.unique);
		const { data } = (await this.#datalistDataSource?.requestItems(uniques)) ?? {};
		if (request !== this.#facetOptionsRequest) return;

		this.#options.setValue(
			facets.map((facet) => {
				const item = data?.find((item) => item.unique === facet.unique);
				return { ...facet, name: item?.name, icon: item?.icon };
			}),
		);
	}

	async #requestValueItems(uniques: Array<string>) {
		if (uniques.length === 0) {
			this.#valueItems.setValue([]);
			return;
		}

		if (!this.#datalistDataSource) return;

		const { data, asObservable } = await this.#datalistDataSource.requestItems(uniques);

		if (asObservable) {
			this.observe(asObservable(), (items) => this.#valueItems.setValue(items ?? []), ObserveValueItems);
		} else if (data) {
			this.#valueItems.setValue(data);
		}
	}

	async #requestOptions() {
		if (!this.#datalistDataSource) return;

		const { data } = await this.#datalistDataSource.requestOptions({
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

export { UmbSelectFacetFilterApi as api };
