import { UmbUserStateFilter } from '../../utils/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_COLLECTION_CONTEXT,
	type ManifestCollectionFilter,
	type UmbCollectionFilterApi,
	type UmbSelectOption,
} from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserStateCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	public manifest?: ManifestCollectionFilter;

	constructor(host: UmbControllerHost) {
		super(host);
		const options = Object.values(UmbUserStateFilter).map((state) => ({
			label: state,
			value: state,
		}));
		this.#options.setValue(options);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	public async setValue(values: Array<string>) {
		this.#value.setValue(values);
		const alias = this.manifest?.alias;
		if (alias) {
			if (values.length === 0) {
				await this.#collectionContext?.filtering.removeFilter(alias);
				this.#collectionContext?.loadCollection();
			} else {
				await this.#collectionContext?.filtering.setFilter({
					alias,
					value: values,
				});
				this.#collectionContext?.loadCollection();
			}
		}
	}
}

export { UmbUserStateCollectionFilterApi as api };
