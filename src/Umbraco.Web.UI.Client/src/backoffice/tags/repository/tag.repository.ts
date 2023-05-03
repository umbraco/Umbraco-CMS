import { UmbTagServerDataSource } from './sources/tag.server.data';
import { UmbTagStore, UMB_TAG_STORE_CONTEXT_TOKEN } from './tag.store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { TagResponseModel, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

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
		{ query, skip, take, tagGroup, culture } = { query: '', skip: 0, take: 1000, tagGroup: 'default', culture: '' }
	) {
		await this.#init;

		const { data, error } = await this.#dataSource.getCollection({ query, skip, take, tagGroup, culture });

		if (data) {
			this.#tagStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#tagStore?.byGroup(tagGroup) };
	}
}
