import { UmbLogViewerRepository } from '../repository/log-viewer.repository.js';
import {
	UmbBasicState,
	UmbArrayState,
	createObservablePart,
	UmbDeepState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import {
	DirectionModel,
	LogLevelCountsReponseModel,
	LogLevelModel,
	PagedLoggerResponseModel,
	PagedLogMessageResponseModel,
	PagedLogTemplateResponseModel,
	PagedSavedLogSearchResponseModel,
	SavedLogSearchPresenationBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { query } from '@umbraco-cms/backoffice/router';

export type PoolingInterval = 0 | 2000 | 5000 | 10000 | 20000 | 30000;
export interface PoolingCOnfig {
	enabled: boolean;
	interval: PoolingInterval;
}
export interface LogViewerDateRange {
	startDate: string;
	endDate: string;
}

export class UmbLogViewerWorkspaceContext extends UmbBaseController {
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

	#savedSearches = new UmbObjectState<PagedSavedLogSearchResponseModel | undefined>(undefined);
	savedSearches = createObservablePart(this.#savedSearches, (data) => data?.items);

	#logCount = new UmbDeepState<LogLevelCountsReponseModel | null>(null);
	logCount = createObservablePart(this.#logCount, (data) => data);

	#dateRange = new UmbDeepState<LogViewerDateRange>(this.defaultDateRange);
	dateRange = createObservablePart(this.#dateRange, (data) => data);

	#loggers = new UmbDeepState<PagedLoggerResponseModel | null>(null);
	loggers = createObservablePart(this.#loggers, (data) => data?.items);

	#canShowLogs = new UmbBasicState<boolean | null>(null);
	canShowLogs = createObservablePart(this.#canShowLogs, (data) => data);

	#isLoadingLogs = new UmbBasicState<boolean | null>(null);
	isLoadingLogs = createObservablePart(this.#isLoadingLogs, (data) => data);

	#filterExpression = new UmbStringState<string>('');
	filterExpression = createObservablePart(this.#filterExpression, (data) => data);

	#messageTemplates = new UmbDeepState<PagedLogTemplateResponseModel | null>(null);
	messageTemplates = createObservablePart(this.#messageTemplates, (data) => data);

	#logLevelsFilter = new UmbArrayState<LogLevelModel>([]);
	logLevelsFilter = createObservablePart(this.#logLevelsFilter, (data) => data);

	#logs = new UmbDeepState<PagedLogMessageResponseModel | null>(null);
	logs = createObservablePart(this.#logs, (data) => data?.items);
	logsTotal = createObservablePart(this.#logs, (data) => data?.total);

	#polling = new UmbObjectState<PoolingCOnfig>({ enabled: false, interval: 2000 });
	polling = createObservablePart(this.#polling, (data) => data);

	#sortingDirection = new UmbBasicState<DirectionModel>(DirectionModel.ASCENDING);
	sortingDirection = createObservablePart(this.#sortingDirection, (data) => data);

	#intervalID: number | null = null;

	currentPage = 1;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.provideContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, this);
		this.#repository = new UmbLogViewerRepository(host);
	}

	onChangeState = () => {
		const searchQuery = query();
		let sanitizedQuery = '';
		if (searchQuery.lq) {
			sanitizedQuery = decodeURIComponent(searchQuery.lq);
		}
		this.setFilterExpression(sanitizedQuery);

		let validLogLevels: LogLevelModel[] = [];
		if (searchQuery.loglevels) {
			const loglevels = searchQuery.loglevels.split(',') as LogLevelModel[];

			// Filter out invalid log levels that do not exist in LogLevelModel
			validLogLevels = loglevels.filter((loglevel) => {
				return Object.values(LogLevelModel).includes(loglevel);
			});
		}
		this.setLogLevelsFilter(validLogLevels);

		const dateRange: Partial<LogViewerDateRange> = {};

		if (searchQuery.startDate) {
			dateRange.startDate = searchQuery.startDate;
		}

		if (searchQuery.endDate) {
			dateRange.endDate = searchQuery.endDate;
		}

		this.setDateRange(dateRange);

		this.setCurrentPage(searchQuery.page ? Number(searchQuery.page) : 1);

		this.getLogs();
	};

	setDateRange(dateRange: Partial<LogViewerDateRange>) {
		let { startDate, endDate } = dateRange;

		if (!startDate) startDate = this.defaultDateRange.startDate;
		if (!endDate) endDate = this.defaultDateRange.endDate;

		const isAnyDateInTheFuture = new Date(startDate) > new Date() || new Date(endDate) > new Date();
		const isStartDateBiggerThenEndDate = new Date(startDate) > new Date(endDate);
		if (isAnyDateInTheFuture || isStartDateBiggerThenEndDate) {
			return;
		}

		this.#dateRange.next({ startDate, endDate });
		this.validateLogSize();
		this.getLogCount();
		this.getMessageTemplates(0, 10);
	}

	async getSavedSearches() {
		const { data } = await this.#repository.getSavedSearches({ skip: 0, take: 100 });
		if (data) {
			this.#savedSearches.next(data);
		} else {
			//falback to some default searches resembling Umbraco <= 12
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

	async saveSearch({ name, query }: SavedLogSearchPresenationBaseModel) {
		const previousSavedSearches = this.#savedSearches.getValue()?.items ?? [];
		try {
			this.#savedSearches.update({ items: [...previousSavedSearches, { name, query }] });
			await this.#repository.saveSearch({ name, query });
		} catch (err) {
			this.#savedSearches.update({ items: previousSavedSearches });
		}
	}

	async removeSearch({ name }: { name: string }) {
		const previousSavedSearches = this.#savedSearches.getValue()?.items ?? [];
		try {
			this.#savedSearches.update({ items: previousSavedSearches.filter((search) => search.name !== name) });
			await this.#repository.removeSearch({ name });
		} catch (err) {
			this.#savedSearches.update({ items: previousSavedSearches });
		}
	}

	async getLogCount() {
		const { data } = await this.#repository.getLogCount({ ...this.#dateRange.getValue() });

		if (data) {
			this.#logCount.next(data);
		}
	}

	async getMessageTemplates(skip: number, take: number) {
		const { data } = await this.#repository.getMessageTemplates({ skip, take, ...this.#dateRange.getValue() });

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
		const { error } = await this.#repository.getLogViewerValidateLogsSize({ ...this.#dateRange.getValue() });
		if (error) {
			this.#canShowLogs.next(false);
			return;
		}
		this.#canShowLogs.next(true);
	}

	setCurrentPage(page: number) {
		this.currentPage = page;
	}

	getLogs = async () => {
		if (this.#canShowLogs.getValue() === false) {
			return;
		}

		const isPollingEnabled = this.#polling.getValue().enabled;

		if (!isPollingEnabled) this.#isLoadingLogs.next(true);

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
		this.#isLoadingLogs.next(false);
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
		this.#polling.update({ interval });
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
