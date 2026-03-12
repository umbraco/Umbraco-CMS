import { UMB_COLLECTION_CONTEXT } from '../../../default/index.js';
import type { ManifestCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import type { UmbSelectOption, MetaCollectionFacetFilterSelect } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDatalistDataSource, UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';

const ObserveValueItems = Symbol();

export class UmbSelectCollectionFacetFilterApi extends UmbControllerBase {
	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#valueItems = new UmbArrayState<UmbDatalistItemModel>([], (x) => x.unique);
	public readonly valueItems = this.#valueItems.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;
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

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeFilterValue();
		});
	}

	#observeFilterValue() {
		const alias = this.#manifest?.alias;
		if (!alias) return;
		this.observe(
			this.#collectionContext?.filtering.filterValueByAlias(alias),
			(activeFilter) => {
				const values = activeFilter ? activeFilter.value : [];
				this.#value.setValue(values);
				this.#requestValueItems(values);
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

	public setValue(values: Array<string>) {
		const filtered = values.filter((v) => v !== '');
		const alias = this.#manifest?.alias;
		if (!alias) return;

		if (filtered.length === 0) {
			this.#collectionContext?.filtering.clearFilter(alias);
		} else {
			this.#collectionContext?.filtering.setFilter({ alias, value: filtered });
		}

		this.#collectionContext?.loadCollection();
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
			const newOptions = data.items.map((item) => ({
				label: item.name ?? '',
				value: item.unique,
			}));

			const currentOptions = this.#options.getValue();
			this.#options.setValue([...currentOptions, ...newOptions]);

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
