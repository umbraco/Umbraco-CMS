import { UmbTagServerDataSource } from './sources/tag.server.data';
import { UmbTagStore, UMB_TAG_STORE_CONTEXT_TOKEN } from './tag.store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

export class UmbTagRepository {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#dataSource: UmbTagServerDataSource;
	#tagStore?: UmbTagStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		this.#dataSource = new UmbTagServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_TAG_STORE_CONTEXT_TOKEN, (instance) => {
				this.#tagStore = instance;
			}).asPromise(),
		]);
	}

	async requestTags(
		tagGroupName: string,
		culture: string | null,
		{ skip, take, query } = { skip: 0, take: 1000, query: '' }
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
			// TODO: append culture? "Invariant" if null.
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
		{ skip, take } = { skip: 0, take: 1000 }
	) {
		return this.requestTags(tagGroupName, culture, { skip, take, query });
	}
}
