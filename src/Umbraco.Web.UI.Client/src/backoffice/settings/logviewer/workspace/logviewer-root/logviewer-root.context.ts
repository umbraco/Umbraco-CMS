import { createObservablePart, DeepState } from '@umbraco-cms/observable-api';
import { PagedSavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbLogSearchRepository } from '../data/log-search.repository';

export class UmbLogViewerWorkspaceContext {
	#host: UmbControllerHostInterface;
	#repository: UmbLogSearchRepository;

	#data = new DeepState<PagedSavedLogSearchModel | undefined>(undefined);
	data = this.#data.asObservable();
	savedSearches = createObservablePart(this.#data, (data) => data?.items);

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#repository = new UmbLogSearchRepository(this.#host);
	}

	async getSavedSearches() {
		const { data } = await this.#repository.getSavedSearches({ skip: 0, take: 100 });

		if (data) {
			this.#data.next(data);
		}
	}
}
