import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterApi, UmbSelectOption } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbUserStateFilter, type UmbUserStateFilterType } from '../../utils';
import { UMB_USER_COLLECTION_CONTEXT } from '../../user-collection.context-token';

export class UmbUserStateCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x.value);
	public readonly options = this.#options.asObservable();

	#collectionContext?: typeof UMB_USER_COLLECTION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);
		const options = Object.values(UmbUserStateFilter).map((state) => ({
			label: state,
			value: state,
		}));
		this.#options.setValue(options);

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	public setValue(values: Array<string>) {
		this.#value.setValue(values);
		this.#collectionContext?.setStateFilter(values as Array<UmbUserStateFilterType>);
	}
}

export { UmbUserStateCollectionFilterApi as api };
