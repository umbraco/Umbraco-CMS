import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterApi, UmbSelectOption } from '@umbraco-cms/backoffice/collection';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbUserGroupCollectionRepository, type UmbUserGroupDetailModel } from '@umbraco-cms/backoffice/user-group';
import { UMB_USER_COLLECTION_CONTEXT } from '../../user-collection.context-token';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x);
	public readonly options = this.#options.asObservable();

	#collectionContext?: typeof UMB_USER_COLLECTION_CONTEXT.TYPE;
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);
		this.#requestUserGroups();

		this.consumeContext(UMB_USER_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
		});
	}

	async #requestUserGroups() {
		const { data } = await this.#userGroupCollectionRepository.requestCollection();
		if (data) {
			const options = data.items.map((group) => ({
				label: group.name ?? '',
				value: group.unique,
			}));
			this.#options.setValue(options);
		}
	}

	public setValue(values: Array<string>) {
		this.#value.setValue(values);
		this.#collectionContext?.setUserGroupFilter(values);
	}
}

export { UmbUserGroupCollectionFilterApi as api };
