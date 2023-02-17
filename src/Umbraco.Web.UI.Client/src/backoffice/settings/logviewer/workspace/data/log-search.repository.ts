import { UmbLogMessagesServerDataSource, UmbLogSearchesServerDataSource } from './sources/log-viewer.server.data';
import { UmbLogSearchesStore, UMB_LOG_SEARCHES_STORE_CONTEXT_TOKEN } from './log-search.store';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbLogSearchRepository {
	#host: UmbControllerHostInterface;
	#searchDataSource: UmbLogSearchesServerDataSource;
	#messagesDataSource: UmbLogMessagesServerDataSource;
	#searchStore?: UmbLogSearchesStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#searchDataSource = new UmbLogSearchesServerDataSource(this.#host);
		this.#messagesDataSource = new UmbLogMessagesServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_LOG_SEARCHES_STORE_CONTEXT_TOKEN, (instance) => {
			this.#searchStore = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
			this.#checkIfInitialized();
		});
	}

	#init() {
		// TODO: This would only works with one user of this method. If two, the first one would be forgotten, but maybe its alright for now as I guess this is temporary.
		return new Promise<void>((resolve) => {
			this.#initialized ? resolve() : (this.#initResolver = resolve);
		});
	}

	#checkIfInitialized() {
		if (this.#searchStore && this.#notificationService) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	async getSavedSearches({ skip, take }: { skip: number; take: number }) {
		await this.#init();

		return this.#searchDataSource.getAllSavedSearches({ skip, take });
	}

	async getLogCount({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		await this.#init();

		return this.#messagesDataSource.getLogViewerLevelCount({ startDate, endDate });
	}

	// async insert(template: Template) {
	// 	await this.#init();

	// 	// TODO: should we show a notification if the template is missing?
	// 	// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
	// 	if (!template) {
	// 		const error: ProblemDetails = { title: 'Template is missing' };
	// 		return { error };
	// 	}

	// 	const { error } = await this.#dataSource.insert(template);

	// 	if (!error) {
	// 		const notification = { data: { message: `Template created` } };
	// 		this.#notificationService?.peek('positive', notification);
	// 	}

	// 	// TODO: we currently don't use the detail store for anything.
	// 	// Consider to look up the data before fetching from the server
	// 	this.#searchStore?.append(template);
	// 	// TODO: Update tree store with the new item?

	// 	return { error };
	// }
}
