import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDefaultMultiSelectCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	public setValue(values: Array<string>) {
		this.#value.setValue(values);
	}
}

export { UmbDefaultMultiSelectCollectionFilterApi as api };
