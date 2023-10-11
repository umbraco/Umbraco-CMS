import { UMB_USER_STORE_CONTEXT_TOKEN, UmbUserStore } from './user.store.js';
import { UMB_USER_ITEM_STORE_CONTEXT_TOKEN, UmbUserItemStore } from './user-item.store.js';
import { UmbUserEnableServerDataSource } from './sources/user-enable.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

export class UmbEnableUserRepository {
	#host: UmbControllerHostElement;
	#init;

	#enableSource: UmbUserEnableServerDataSource;
	#detailStore?: UmbUserStore;
	#itemStore?: UmbUserItemStore;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#enableSource = new UmbUserEnableServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_USER_STORE_CONTEXT_TOKEN, (instance) => {
				this.#detailStore = instance;
			}).asPromise(),

			new UmbContextConsumerController(this.#host, UMB_USER_ITEM_STORE_CONTEXT_TOKEN, (instance) => {
				this.#itemStore = instance;
			}).asPromise(),
		]);
	}

	async enable(ids: Array<string>) {
		if (ids.length === 0) throw new Error('User ids are missing');
		await this.#init;

		const { error } = await this.#enableSource.enable(ids);
	}
}
