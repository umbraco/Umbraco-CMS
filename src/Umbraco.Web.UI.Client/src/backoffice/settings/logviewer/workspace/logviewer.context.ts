import { UmbLogViewerRepository } from './data/log-viewer.repository';
import { ArrayState, createObservablePart, DeepState, StringState } from '@umbraco-cms/observable-api';
import {
	DirectionModel,
	LoggerModel,
	LogLevelModel,
	PagedLoggerModel,
	PagedLogMessageModel,
	PagedLogTemplateModel,
	PagedSavedLogSearchModel,
} from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextToken } from '@umbraco-cms/context-api';

const logLevels = {
	Information: 171,
	Debug: 39,
	Warning: 31,
	Error: 1,
	Fatal: 0,
};

export type LogLevel = Record<keyof typeof logLevels, number>;

export interface LogViewerDateRange {
	startDate: string;
	endDate: string;
}

export class UmbLogViewerWorkspaceContext {
	#host: UmbControllerHostInterface;
	#repository: UmbLogViewerRepository;

	get today() {
		const today = new Date();
		const dd = String(today.getDate()).padStart(2, '0');
		const mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
		const yyyy = today.getFullYear();

		return yyyy + '-' + mm + '-' + dd;
	}

	get yesterday() {
		const today = new Date();
		const dd = String(today.getDate() - 1).padStart(2, '0');
		const mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
		const yyyy = today.getFullYear();

		return yyyy + '-' + mm + '-' + dd;
	}

	defaultDateRange: LogViewerDateRange = {
		startDate: this.yesterday,
		endDate: this.today,
	};

	#savedSearches = new DeepState<PagedSavedLogSearchModel | undefined>(undefined);
	savedSearches = createObservablePart(this.#savedSearches, (data) => data?.items);

	#logCount = new DeepState<LogLevel | null>(null);
	logCount = createObservablePart(this.#logCount, (data) => data);

	#dateRange = new DeepState<LogViewerDateRange>(this.defaultDateRange);
	dateRange = createObservablePart(this.#dateRange, (data) => data);

	#currentQuery = new StringState<string>('');
	currentQuery = createObservablePart(this.#currentQuery, (data) => data);

	#messageTemplates = new DeepState<PagedLogTemplateModel | null>(null);
	messageTemplates = createObservablePart(this.#messageTemplates, (data) => data);

	#logLevel = new ArrayState<LogLevelModel>([]);
	logLevel = createObservablePart(this.#logLevel, (data) => data);

	#logs = new DeepState<PagedLogMessageModel | null>(null);
	logs = createObservablePart(this.#logs, (data) => data?.items);

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		this.#repository = new UmbLogViewerRepository(this.#host);
	}

	async init() {
		try {
			await Promise.all([
				this.getMessageTemplates(0, 10),
				this.getLogCount(this.defaultDateRange),
				this.getSavedSearches(),
			]);
		} catch (error) {
			console.error(error);
		}
	}

	setDateRange(dateRange: LogViewerDateRange) {
		this.#dateRange.next(dateRange);
		this.getLogCount(dateRange);
	}

	async getSavedSearches() {
		const { data } = await this.#repository.getSavedSearches({ skip: 0, take: 100 });
		if (data) {
			this.#savedSearches.next(data);
		}
	}

	async getLogCount({ startDate, endDate }: LogViewerDateRange) {
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

	async getLogs() {
		const options = {
			orderDirection: DirectionModel.ASCENDING,
			filterExpression: this.#currentQuery.getValue(),
			logLevel: this.#logLevel.getValue(),
			...this.#dateRange.getValue(),
		};

		const { data } = await this.#repository.getLogs(options);

		if (data) {
			this.#logs.next(data);
		}
	}

	setCurrentQuery(query: string) {
		this.#currentQuery.next(query);
	}
}

export const UMB_APP_LOG_VIEWER_CONTEXT_TOKEN = new UmbContextToken<UmbLogViewerWorkspaceContext>(
	UmbLogViewerWorkspaceContext.name
);
