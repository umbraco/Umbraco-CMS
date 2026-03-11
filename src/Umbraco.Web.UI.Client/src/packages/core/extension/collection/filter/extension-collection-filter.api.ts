import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_COLLECTION_CONTEXT,
	type ManifestCollectionFilter,
	type UmbCollectionFilterApi,
	type UmbSelectOption,
} from '@umbraco-cms/backoffice/collection';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';

export class UmbExtensionCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;
	public manifest?: ManifestCollectionFilter;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;

			this.observe(instance?.filtering.activeFilters, (activeFilters) => {
				const alias = this.manifest?.alias;
				if (alias && !activeFilters?.find((f) => f.alias === alias)) {
					this.#value.setValue([]);
				}
			});
		});

		this.observe(umbExtensionsRegistry.extensions, (extensions) => {
			const types = [...new Set(extensions.map((x) => x.type))];
			const options = types.sort().map((x) => ({ label: fromCamelCase(x), value: x }));
			this.#options.setValue([...options]);
		});
	}

	public async setValue(values: Array<string>) {
		const filtered = values.filter((v) => v !== '');
		this.#value.setValue(values);
		const alias = this.manifest?.alias;
		if (alias) {
			if (filtered.length === 0) {
				await this.#collectionContext?.filtering.removeFilter(alias);
				this.#collectionContext?.loadCollection();
			} else {
				await this.#collectionContext?.filtering.setFilter({
					alias,
					value: filtered,
				});
				this.#collectionContext?.loadCollection();
			}
		}
	}
}

export { UmbExtensionCollectionFilterApi as api };
