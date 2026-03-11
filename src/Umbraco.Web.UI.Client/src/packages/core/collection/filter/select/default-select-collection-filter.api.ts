import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestCollectionFilter } from '../collection-filter.extension.js';

export class UmbDefaultSelectCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#selection = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#selection.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	public manifest?: ManifestCollectionFilter;

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
				this.#selection.setValue([value[filterKey]]);
			}
		});
	}

	public setValue(values: Array<string>) {
		this.#selection.setValue(values);
		const filterKey = this.manifest?.meta?.filterKey;
		if (filterKey) {
			this.#collectionContext?.setFilter({ [filterKey]: values[0] });
		}
	}
}

export { UmbDefaultSelectCollectionFilterApi as api };
