import type { ManifestFilter } from '../filter.extension.js';
import type { MetaFilterSelect } from './types.js';
import { UmbSelectFilterApiBase } from './select-filter-api.base.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';

const ObserveValueItems = Symbol();

/**
 * API of the `select` filter kind: options and the items of the selected values come from the filter's datalist data source.
 */
export class UmbSelectFilterApi extends UmbSelectFilterApiBase {
	#datalistDataSource?: UmbDatalistDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.pagination.setPageSize(100);
	}

	public override loadOptions() {
		this.#requestOptions();
	}

	public override loadMoreOptions() {
		const nextPage = this.pagination.getCurrentPageNumber() + 1;
		if (nextPage <= this.pagination.getTotalPages()) {
			this.pagination.setCurrentPageNumber(nextPage);
			this.#requestOptions();
		}
	}

	protected override _onManifest(manifest: ManifestFilter | undefined) {
		const meta = manifest?.meta as MetaFilterSelect | undefined;
		if (meta?.datalistDataSource) {
			this.#datalistDataSource = new meta.datalistDataSource(this);
		}
	}

	protected override async _requestValueItems(uniques: Array<string>) {
		if (uniques.length === 0) {
			this._setValueItems([]);
			return;
		}

		if (!this.#datalistDataSource) return;

		const { data, asObservable } = await this.#datalistDataSource.requestItems(uniques);

		if (asObservable) {
			this.observe(asObservable(), (items) => this._setValueItems(items ?? []), ObserveValueItems);
		} else if (data) {
			this._setValueItems(data);
		}
	}

	async #requestOptions() {
		if (!this.#datalistDataSource) return;

		const { data } = await this.#datalistDataSource.requestOptions({
			skip: this.pagination.getSkip(),
			take: this.pagination.getPageSize(),
		});

		if (data) {
			this._setOptions([...this._getOptions(), ...data.items]);
			this.pagination.setTotalItems(data.total);
		} else {
			if (this.pagination.getCurrentPageNumber() === 1) {
				this.pagination.clear();
				this._setOptions([]);
			}
		}
	}
}

export { UmbSelectFilterApi as api };
