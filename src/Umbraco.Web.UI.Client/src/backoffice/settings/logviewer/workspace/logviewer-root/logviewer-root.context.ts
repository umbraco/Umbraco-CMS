import { UmbLogViewerRepository } from '../data/log-viewer.repository';
import { createObservablePart, DeepState } from '@umbraco-cms/observable-api';
import { PagedLogTemplateModel, PagedSavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextToken } from '@umbraco-cms/context-api';

const logLevels = {
	information: 171,
	debug: 39,
	warning: 31,
	error: 1,
	fatal: 0,
};

export type LogLevel = Record<keyof typeof logLevels, number>;

export class UmbLogViewerWorkspaceContext {
	#host: UmbControllerHostInterface;
	#repository: UmbLogViewerRepository;

	#savedSearches = new DeepState<PagedSavedLogSearchModel | undefined>(undefined);
	savedSearches = createObservablePart(this.#savedSearches, (data) => data?.items);

	#logCount = new DeepState<LogLevel | null>(null);
	logCount = createObservablePart(this.#logCount, (data) => data);

	#messageTemplates = new DeepState<PagedLogTemplateModel | null>(null);
	messageTemplates = createObservablePart(this.#messageTemplates, (data) => data);

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#repository = new UmbLogViewerRepository(this.#host);
	}

	async getSavedSearches() {
		const { data } = await this.#repository.getSavedSearches({ skip: 0, take: 100 });

		if (data) {
			this.#savedSearches.next(data);
		}
	}

	async getLogCount(startDate: string, endDate: string) {
		const { data } = await this.#repository.getLogCount({ startDate, endDate });

		if (data) {
			this.#logCount.next(data);
		}
	}

	async getMessageTemplates(skip: number, take: number) {
		const { data } = await this.#repository.getMessageTemplates({ skip, take });

		if (data) {
			this.#messageTemplates.next(data);
		}
	}
}

export const UMB_APP_LOG_VIEWER_CONTEXT_TOKEN = new UmbContextToken<UmbLogViewerWorkspaceContext>(
	UmbLogViewerWorkspaceContext.name
);
