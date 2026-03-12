import type { UmbCollectionFacetFilterApi, UmbSelectOption } from '../collection-facet-filter-api.interface.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDefaultMultiSelectCollectionFacetFilterApi
	extends UmbControllerBase
	implements UmbCollectionFacetFilterApi
{
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	public setValue(values: Array<string>) {
		this.#value.setValue(values);
	}
}

export { UmbDefaultMultiSelectCollectionFacetFilterApi as api };
