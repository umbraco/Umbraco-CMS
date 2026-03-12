import { UmbUserStateFilter } from '../../utils/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_COLLECTION_CONTEXT,
	type ManifestCollectionFacetFilter,
	type UmbCollectionFacetFilterApi,
	type UmbSelectOption,
} from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbDatalistItemModel } from '@umbraco-cms/backoffice/datalist-data-source';

export class UmbUserStateCollectionFilterApi extends UmbControllerBase implements UmbCollectionFacetFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	#valueItems = new UmbArrayState<UmbDatalistItemModel>([], (x) => x.unique);
	public readonly valueItems = this.#valueItems.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	public manifest?: ManifestCollectionFacetFilter;

	constructor(host: UmbControllerHost) {
		super(host);
		const options = Object.values(UmbUserStateFilter).map((state) => ({
			label: state,
			value: state,
		}));
		this.#options.setValue(options);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeFilterValue();
		});
	}

	#observeFilterValue() {
		const alias = this.manifest?.alias;
		if (!alias) return;
		this.observe(
			this.#collectionContext?.filtering.filterValueByAlias(alias),
			(activeFilter) => {
				const values: Array<string> = activeFilter ? activeFilter.value : [];
				this.#value.setValue(values);
				const options = this.#options.getValue();
				this.#valueItems.setValue(
					values.map((v) => {
						const option = options.find((o) => o.value === v);
						return { unique: v, name: option?.label ?? v };
					}),
				);
			},
			'umbFilterValueObserver',
		);
	}

	public setValue(values: Array<string>) {
		const alias = this.manifest?.alias;
		if (!alias) return;

		if (values.length === 0) {
			this.#collectionContext?.filtering.clearFilter(alias);
		} else {
			this.#collectionContext?.filtering.setFilter({ alias, value: values });
		}

		this.#collectionContext?.loadCollection();
	}
}

export { UmbUserStateCollectionFilterApi as api };
