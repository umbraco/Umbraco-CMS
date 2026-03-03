import type { UmbCollectionFilterApi } from '../collection-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDefaultCollectionSelectFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#selection = new UmbArrayState<string>([], (x) => x);
	public readonly selection = this.#selection.asObservable();

	public setSelection(values: Array<string>) {
		this.#selection.setValue(values);
	}
}

export { UmbDefaultCollectionSelectFilterApi as api };
