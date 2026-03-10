import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDefaultCollectionMultiSelectFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#selection = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#selection.asObservable();

	public setValue(values: Array<string>) {
		this.#selection.setValue(values);
	}
}

export { UmbDefaultCollectionMultiSelectFilterApi as api };
