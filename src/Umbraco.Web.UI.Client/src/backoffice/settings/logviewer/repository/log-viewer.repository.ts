import { UmbLogMessagesServerDataSource, UmbLogSearchesServerDataSource } from './sources/log-viewer.server.data';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { DirectionModel, LogLevelModel } from '@umbraco-cms/backoffice/backend-api';

// Move to documentation / JSdoc
/* We need to create a new instance of the repository from within the element context. We want the notifications to be displayed in the right context. */
// element -> context -> repository -> (store) -> data source
// All methods should be async and return a promise. Some methods might return an observable as part of the promise response.
export class UmbLogViewerRepository {
	#host: UmbControllerHostElement;
	#searchDataSource: UmbLogSearchesServerDataSource;
	#messagesDataSource: UmbLogMessagesServerDataSource;
	#notificationService?: UmbNotificationContext;
	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#searchDataSource = new UmbLogSearchesServerDataSource(this.#host);
		this.#messagesDataSource = new UmbLogMessagesServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
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
		if (this.#notificationService) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	async getSavedSearches({ skip, take }: { skip: number; take: number }) {
		await this.#init();

		return this.#searchDataSource.getAllSavedSearches({ skip, take });
	}

	async getMessageTemplates({ skip, take }: { skip: number; take: number }) {
		await this.#init();

		return this.#messagesDataSource.getLogViewerMessageTemplate({ skip, take });
	}

	async getLogCount({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		await this.#init();

		return this.#messagesDataSource.getLogViewerLevelCount({ startDate, endDate });
	}

	async getLogs({
		skip = 0,
		take = 100,
		orderDirection,
		filterExpression,
		logLevel,
		startDate,
		endDate,
	}: {
		skip?: number;
		take?: number;
		orderDirection?: DirectionModel;
		filterExpression?: string;
		logLevel?: Array<LogLevelModel>;
		startDate?: string;
		endDate?: string;
	}) {
		await this.#init();

		return this.#messagesDataSource.getLogViewerLogs({
			skip,
			take,
			orderDirection,
			filterExpression,
			logLevel,
			startDate,
			endDate,
		});
	}

	async getLogLevels({ skip = 0, take = 100 }: { skip: number; take: number }) {
		await this.#init();
		return this.#messagesDataSource.getLogViewerLevel({ skip, take });
	}

	async getLogViewerValidateLogsSize({ startDate, endDate }: { startDate?: string; endDate?: string }) {
		await this.#init();
		return this.#messagesDataSource.getLogViewerValidateLogsSize({ startDate, endDate });
	}
}
