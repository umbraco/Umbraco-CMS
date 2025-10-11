import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbNewsServerDataSource } from './sources/umbraco-news.server.data';

export class UmbNewsDashboardRepository {
	#host: UmbControllerHost;
	#newsDataSource: UmbNewsServerDataSource;

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#newsDataSource = new UmbNewsServerDataSource(this.#host);
	}

	async getNewsDashboard() {
		return this.#newsDataSource.getNewsItems();
	}
}
