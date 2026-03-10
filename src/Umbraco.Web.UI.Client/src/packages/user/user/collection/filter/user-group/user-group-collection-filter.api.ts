import { UMB_USER_COLLECTION_CONTEXT } from '../../user-collection.context-token.js';
import { UmbUserGroupDatalistDataSource } from './user-group-datalist-data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterApi, UmbSelectOption } from '@umbraco-cms/backoffice/collection';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type { UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';

const ObserveValueItems = Symbol();

export class UmbUserGroupCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x);
	public readonly options = this.#options.asObservable();

	#valueItems = new UmbArrayState<UmbDatalistItemModel>([], (x) => x.unique);
	public readonly valueItems = this.#valueItems.asObservable();

	#collectionContext?: typeof UMB_USER_COLLECTION_CONTEXT.TYPE;
	#datalistDataSource = new UmbUserGroupDatalistDataSource(this);
	public readonly pagination = new UmbPaginationManager();

	constructor(host: UmbControllerHost) {
		super(host);
		this.pagination.setPageSize(20);

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
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
		this.#value.setValue(values);
		this.#collectionContext?.setUserGroupFilter(values);
		this.#requestValueItems(values);
	}

	async #requestValueItems(uniques: Array<string>) {
		if (uniques.length === 0) {
			this.#valueItems.setValue([]);
			return;
		}

		const { data, asObservable } = await this.#datalistDataSource.requestItems(uniques);

		if (asObservable) {
			this.observe(asObservable(), (items) => this.#valueItems.setValue(items), ObserveValueItems);
		} else if (data) {
			this.#valueItems.setValue(data);
		}
	}

	async #requestOptions() {
		const { data } = await this.#datalistDataSource.requestOptions({
			skip: this.pagination.getSkip(),
			take: this.pagination.getPageSize(),
		});

		if (data) {
			const newOptions = data.items.map((group) => ({
				label: group.name ?? '',
				value: group.unique,
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

export { UmbUserGroupCollectionFilterApi as api };
