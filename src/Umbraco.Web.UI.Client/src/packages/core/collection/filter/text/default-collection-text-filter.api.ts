import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { debounce } from '@umbraco-cms/backoffice/utils';

export class UmbDefaultCollectionTextFilterApi extends UmbControllerBase implements UmbCollectionTextFilterApi {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	public updateTextFilter(value: string) {
		this.#debouncedSearch(value);
	}

	#debouncedSearch = debounce((value: string) => this.#collectionContext?.setFilter({ filter: value }), 500);

	override destroy() {
		this.#debouncedSearch.cancel();
		super.destroy();
	}
}

export { UmbDefaultCollectionTextFilterApi as api };
