import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_COLLECTION_CONTEXT,
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

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});

		this.observe(umbExtensionsRegistry.extensions, (extensions) => {
			const types = [...new Set(extensions.map((x) => x.type))];
			const options = types.sort().map((x) => ({ label: fromCamelCase(x), value: x }));
			this.#options.setValue([{ label: 'All', value: 'all' }, ...options]);
		});
	}

	public setValue(values: Array<string>) {
		const filtered = values.filter((v) => v !== 'all' && v !== '');
		console.log('filtered values:', filtered);
		this.#value.setValue(values);
		this.#collectionContext?.setFilter({ extensionTypes: filtered } as any);
	}
}

export { UmbExtensionCollectionFilterApi as api };
