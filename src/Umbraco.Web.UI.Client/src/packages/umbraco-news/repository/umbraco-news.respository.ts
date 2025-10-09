import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

// Mock data server for now (backend not ready)
import { UmbNewsStoriesMockDataSource } from './sources/umbraco-news.mock.data.js';

//In the log-viewer they use the Models from the backend-api
//to define the types, but here I am not sure where to use it
import type { ServerNewsStory } from './sources/index.js';

export class UmbNewsStoriesRepository {
	#host: UmbControllerHost;
	#mockDS: UmbNewsStoriesMockDataSource;

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#mockDS = new UmbNewsStoriesMockDataSource(this.#host);
	}

	/** Fetch all stories. */
	async getAll() {
		return this.#mockDS.getAllNewsStories();
	}
}
