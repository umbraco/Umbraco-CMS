import { UMB_COLLECTION_CONTEXT } from '../../default/index.js';
import type { UmbCollectionFilterModel } from '../../collection-filter-model.interface.js';
import type { UmbCollectionTextFilterApi } from './collection-text-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';

/**
 * Default API for the collection text filter extension.
 *
 * This API handles updating the collection filter with a debounced text value.
 * It consumes the collection context and applies the filter when the user types.
 * @class UmbDefaultCollectionTextFilterApi
 * @augments {UmbControllerBase}
 * @implements {UmbCollectionTextFilterApi}
 */
export class UmbDefaultCollectionTextFilterApi extends UmbControllerBase implements UmbCollectionTextFilterApi {
	#text = new UmbStringState(undefined);
	public readonly text = this.#text.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeFilter();
		});
	}

	#observeFilter() {
		this.observe(this.#collectionContext?.filter, (value) => {
			const filterText = (value as UmbCollectionFilterModel)?.filter;
			this.#text.setValue(filterText);
		});
	}

	/**
	 * Updates the collection filter with the given text value.
	 * The filter is debounced to avoid excessive updates while the user is typing.
	 * @param {string} value - The text value to filter the collection by.
	 */
	public setText(value: string) {
		this.#debouncedFilter(value);
	}

	#debouncedFilter = debounce((value: string) => this.#collectionContext?.setFilter({ filter: value }), 500);

	override destroy() {
		this.#debouncedFilter.cancel();
		super.destroy();
	}
}

export { UmbDefaultCollectionTextFilterApi as api };
