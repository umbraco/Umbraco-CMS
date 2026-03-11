import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_COLLECTION_CONTEXT,
	type ManifestCollectionFilter,
	type UmbCollectionFilterApi,
	type UmbSelectOption,
} from '@umbraco-cms/backoffice/collection';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbUserGroupCollectionRepository, type UmbUserGroupDetailModel } from '@umbraco-cms/backoffice/user-group';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupCollectionFilterApi extends UmbControllerBase implements UmbCollectionFilterApi {
	#value = new UmbArrayState<string>([], (x) => x);
	public readonly value = this.#value.asObservable();

	#options = new UmbArrayState<UmbSelectOption>([], (x) => x);
	public readonly options = this.#options.asObservable();

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;
	#userGroupCollectionRepository = new UmbUserGroupCollectionRepository(this);

	public manifest?: ManifestCollectionFilter;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#requestUserGroups();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
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

export { UmbUserGroupCollectionFilterApi as api };
