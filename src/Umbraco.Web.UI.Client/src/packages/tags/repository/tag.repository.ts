import { UmbTagServerDataSource } from './sources/tag.server.data.js';
import type { UmbTagStore } from './tag.store.js';
import { UMB_TAG_STORE_CONTEXT } from './tag.store.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbTagRepository extends UmbControllerBase implements UmbApi {
	#init!: Promise<unknown>;

	#dataSource: UmbTagServerDataSource;
	#tagStore?: UmbTagStore;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbTagServerDataSource(this);

		this.#init = this.consumeContext(UMB_TAG_STORE_CONTEXT, (instance) => {
			this.#tagStore = instance;
		}).asPromise({ preventTimeout: true });
	}

	async requestTags(
		tagGroupName: string,
		culture: string | null,
		{ skip, take, query } = { skip: 0, take: 1000, query: '' },
	) {
		await this.#init;

		const requestCulture = culture || '';

		const { data, error } = await this.#dataSource.getCollection({
			skip,
			take,
			tagGroup: tagGroupName,
			culture: requestCulture,
			query,
		});

		if (data) {
			// TODO: allow to append an array of items to the store
			// TODO: Lone: append culture? "Invariant" if null. Niels: Actually, as of my current stand point, I think we should aim for invariant to be the value of ´null´.
			data.items.forEach((x) => this.#tagStore?.append(x));
		}

		return {
			data,
			error,
			asObservable: () => this.#tagStore!.byQuery(tagGroupName, requestCulture, query),
		};
	}

	async queryTags(
		tagGroupName: string,
		culture: string | null,
		query: string,
		{ skip, take } = { skip: 0, take: 1000 },
	) {
		return this.requestTags(tagGroupName, culture, { skip, take, query });
	}
}

export default UmbTagRepository;
