import { UmbLogViewerRepository } from '../repository/log-viewer.repository';
import {
	BasicState,
	ArrayState,
	createObservablePart,
	DeepState,
	ObjectState,
	StringState,
} from '@umbraco-cms/backoffice/observable-api';
import {
	DirectionModel,
	LogLevelCountsReponseModel,
	LogLevelModel,
	PagedLoggerResponseModel,
	PagedLogMessageResponseModel,
	PagedLogTemplateResponseModel,
	PagedSavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export type PoolingInterval = 0 | 2000 | 5000 | 10000 | 20000 | 30000;
export interface PoolingCOnfig {
	enabled: boolean;
	interval: PoolingInterval;
}
export interface LogViewerDateRange {
	startDate: string;
	endDate: string;
}

export class UmbLogViewerWorkspaceContext {
	#host: UmbControllerHostElement;
	#repository: UmbLogViewerRepository;

	get today() {
		const today = new Date();
		const dd = String(today.getDate()).padStart(2, '0');
		const mm = String(today.getMonth() + 1).padStart(2, '0');
		const yyyy = today.getFullYear();

		return yyyy + '-' + mm + '-' + dd;
	}

	get yesterday() {
		const yesterday = new Date(new Date().setDate(new Date().getDate() - 1));
		const dd = String(yesterday.getDate()).padStart(2, '0');
		const mm = String(yesterday.getMonth() + 1).padStart(2, '0');
		const yyyy = yesterday.getFullYear();

		return yyyy + '-' + mm + '-' + dd;
	}

	defaultDateRange: LogViewerDateRange = {
		startDate: this.yesterday,
		endDate: this.today,
	};

	#savedSearches = new DeepState<PagedSavedLogSearchResponseModel | undefined>(undefined);
	savedSearches = createObservablePart(this.#savedSearches, (data) => data?.items);

	#logCount = new DeepState<LogLevelCountsReponseModel | null>(null);
	logCount = createObservablePart(this.#logCount, (data) => data);

	#dateRange = new DeepState<LogViewerDateRange>(this.defaultDateRange);
	dateRange = createObservablePart(this.#dateRange, (data) => data);

	#loggers = new DeepState<PagedLoggerResponseModel | null>(null);
	loggers = createObservablePart(this.#loggers, (data) => data?.items);

	#canShowLogs = new BasicState<boolean | null>(null);
	canShowLogs = createObservablePart(this.#canShowLogs, (data) => data);

	#filterExpression = new StringState<string>('');
	filterExpression = createObservablePart(this.#filterExpression, (data) => data);

	#messageTemplates = new DeepState<PagedLogTemplateResponseModel | null>(null);
	messageTemplates = createObservablePart(this.#messageTemplates, (data) => data);

	#logLevelsFilter = new ArrayState<LogLevelModel>([]);
	logLevelsFilter = createObservablePart(this.#logLevelsFilter, (data) => data);

	#logs = new DeepState<PagedLogMessageResponseModel | null>(null);
	logs = createObservablePart(this.#logs, (data) => data?.items);
	logsTotal = createObservablePart(this.#logs, (data) => data?.total);

	#polling = new ObjectState<PoolingCOnfig>({ enabled: false, interval: 2000 });
	polling = createObservablePart(this.#polling, (data) => data);

	#sortingDirection = new BasicState<DirectionModel>(DirectionModel.ASCENDING);
	sortingDirection = createObservablePart(this.#sortingDirection, (data) => data);

	#intervalID: number | null = null;

	currentPage = 1;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#repository = new UmbLogViewerRepository(this.#host);
	}

	async init() {
		this.validateLogSize();
	}

	setDateRange(dateRange: LogViewerDateRange) {
		const { startDate, endDate } = dateRange;

		const isAnyDateInTheFuture = new Date(startDate) > new Date() || new Date(endDate) > new Date();
		const isStartDateBiggerThenEndDate = new Date(startDate) > new Date(endDate);
		if (isAnyDateInTheFuture || isStartDateBiggerThenEndDate) {
			return;
		}

		this.#dateRange.next(dateRange);
		this.validateLogSize();
		this.getLogCount();
	}

	async getSavedSearches() {
		const { data } = await this.#repository.getSavedSearches({ skip: 0, take: 100 });
		if (data) {
			this.#savedSearches.next(data);
		} else {
			//falback to some default searches like in the old backoffice
			this.#savedSearches.next({
				items: [
					{
						name: 'Find all logs where the Level is NOT Verbose and NOT Debug',
						query: "Not(@Level='Verbose') and Not(@Level='Debug')",
					},
					{
						name: 'Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)',
						query: 'Has(@Exception)',
					},
					{
						name: "Find all logs that have the property 'Duration'",
						query: 'Has(Duration)',
					},
					{
						name: "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
						query: 'Has(Duration) and Duration > 1000',
					},
					{
						name: "Find all logs that are from the namespace 'Umbraco.Core'",
						query: "StartsWith(SourceContext, 'Umbraco.Core')",
					},
					{
						name: 'Find all logs that use a specific log message template',
						query: "@MessageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'",
					},
				],
				total: 6,
			});
		}
	}

	async getLogCount() {
		const { data } = await this.#repository.getLogCount({ ...this.#dateRange.getValue() });

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

	async getLogLevels(skip: number, take: number) {
		const { data } = await this.#repository.getLogLevels({ skip, take });

		if (data) {
			this.#loggers.next(data);
		}
	}

	async validateLogSize() {
		const { data, error } = await this.#repository.getLogViewerValidateLogsSize({ ...this.#dateRange.getValue() });
		if (error) {
			this.#canShowLogs.next(false);
			console.info('LogViewer: ', error);
			return;
		}
		this.#canShowLogs.next(true);
		console.info('LogViewer:showinfg logs');
	}

	setCurrentPage(page: number) {
		this.currentPage = page;
	}

	getLogs = async () => {
		if (!this.#canShowLogs.getValue()) {
			return;
		}

		const skip = (this.currentPage - 1) * 100;
		const take = 100;

		const options = {
			skip,
			take,
			orderDirection: this.#sortingDirection.getValue(),
			filterExpression: this.#filterExpression.getValue(),
			logLevel: this.#logLevelsFilter.getValue(),
			...this.#dateRange.getValue(),
		};

		const { data } = await this.#repository.getLogs(options);

		if (data) {
			this.#logs.next(data);
		}
	};

	setFilterExpression(query: string) {
		this.#filterExpression.next(query);
	}

	setLogLevelsFilter(logLevels: LogLevelModel[]) {
		this.#logLevelsFilter.next(logLevels);
	}

	togglePolling() {
		const isEnabled = !this.#polling.getValue().enabled;
		this.#polling.update({
			enabled: isEnabled,
		});

		if (isEnabled) {
			this.#intervalID = setInterval(() => {
				this.currentPage = 1;
				this.getLogs();
			}, this.#polling.getValue().interval) as unknown as number;
			return;
		}

		clearInterval(this.#intervalID as number);
	}

	setPollingInterval(interval: PoolingInterval) {
		this.#polling.update({ interval, enabled: true });
	}

	toggleSortOrder() {
		const direction = this.#sortingDirection.getValue();
		const newDirection = direction === DirectionModel.ASCENDING ? DirectionModel.DESCENDING : DirectionModel.ASCENDING;
		this.#sortingDirection.next(newDirection);
	}
}

export const UMB_APP_LOG_VIEWER_CONTEXT_TOKEN = new UmbContextToken<UmbLogViewerWorkspaceContext>(
	UmbLogViewerWorkspaceContext.name
);
