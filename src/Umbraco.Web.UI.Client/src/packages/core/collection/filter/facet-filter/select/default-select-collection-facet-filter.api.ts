import type { UmbCollectionFacetFilterApi, UmbSelectOption } from '../collection-facet-filter-api.interface.js';
import { UMB_COLLECTION_CONTEXT } from '../../../default/index.js';
import type { ManifestCollectionFacetFilter } from '../collection-facet-filter.extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultSelectCollectionFacetFilterApi extends UmbControllerBase implements UmbCollectionFacetFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	public manifest?: ManifestCollectionFacetFilter;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeFilter();
		});
	}

	#observeFilter() {
		this.observe(this.#collectionContext?.filter, (value: any) => {
			const filterKey = this.manifest?.meta?.filterKey;
			if (filterKey && value?.[filterKey]) {
				this.#value.setValue([value[filterKey]]);
			}
		});
	}

	public setValue(values: Array<string>) {
		this.#value.setValue(values);
		const filterKey = this.manifest?.meta?.filterKey;
		if (filterKey) {
			this.#collectionContext?.setFilter({ [filterKey]: values[0] });
		}
	}
}

export { UmbDefaultSelectCollectionFacetFilterApi as api };
