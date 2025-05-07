import { UmbLogViewerRepository } from '../repository/log-viewer.repository.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from './logviewer-workspace.context-token.js';
import { UmbBasicState, UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type {
	LogLevelCountsReponseModel,
	PagedLoggerResponseModel,
	PagedLogMessageResponseModel,
	PagedLogTemplateResponseModel,
	PagedSavedLogSearchResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { DirectionModel, LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { query } from '@umbraco-cms/backoffice/router';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

export type PoolingInterval = 0 | 2000 | 5000 | 10000 | 20000 | 30000;
export interface PoolingCOnfig {
	enabled: boolean;
	interval: PoolingInterval;
}
export interface LogViewerDateRange {
	startDate: string;
	endDate: string;
}

// TODO: Revisit usage of workspace for this case...
export class UmbLogViewerWorkspaceContext extends UmbContextBase implements UmbWorkspaceContext {
	public readonly workspaceAlias: string = 'Umb.Workspace.LogViewer';
	#repository: UmbLogViewerRepository;

	getEntityType() {
		return 'log-viewer';
	}

	getEntityName() {
		return 'Log Viewer';
	}

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
	savedSearches = this.#savedSearches.asObservablePart((data) => data);

	#logCount = new UmbObjectState<LogLevelCountsReponseModel | null>(null);
	logCount = this.#logCount.asObservable();

	#dateRange = new UmbObjectState<LogViewerDateRange>(this.defaultDateRange);
	dateRange = this.#dateRange.asObservable();

	#loggers = new UmbObjectState<PagedLoggerResponseModel | null>(null);
	loggers = this.#loggers.asObservablePart((data) => data?.items);

	#canShowLogs = new UmbBasicState<boolean | null>(null);
	canShowLogs = this.#canShowLogs.asObservable();

	#isLoadingLogs = new UmbBasicState<boolean | null>(null);
	isLoadingLogs = this.#isLoadingLogs.asObservable();

	#filterExpression = new UmbStringState<string>('');
	filterExpression = this.#filterExpression.asObservable();

	#messageTemplates = new UmbObjectState<PagedLogTemplateResponseModel | null>(null);
	messageTemplates = this.#messageTemplates.asObservable();

	#logLevelsFilter = new UmbArrayState<LogLevelModel>([], (x) => x);
	logLevelsFilter = this.#logLevelsFilter.asObservable();

	#logs = new UmbObjectState<PagedLogMessageResponseModel | null>(null);
	logs = this.#logs.asObservablePart((data) => data?.items);
	logsTotal = this.#logs.asObservablePart((data) => data?.total);

	#polling = new UmbObjectState<PoolingCOnfig>({ enabled: false, interval: 2000 });
	polling = this.#polling.asObservable();

	#sortingDirection = new UmbBasicState<DirectionModel>(DirectionModel.DESCENDING);
	sortingDirection = this.#sortingDirection.asObservable();

	#intervalID: number | null = null;

	currentPage = 1;

	constructor(host: UmbControllerHost) {
		super(host, UMB_APP_LOG_VIEWER_CONTEXT);
		// TODO: Revisit usage of workspace for this case... currently no other workspace context provides them self with their own token, we need to update UMB_APP_LOG_VIEWER_CONTEXT to become a workspace context. [NL]
		this.provideContext(UMB_WORKSPACE_CONTEXT, this);
		this.#repository = new UmbLogViewerRepository(host);
	}

	override hostConnected() {
		super.hostConnected();
		window.addEventListener('changestate', this.onChangeState);
		this.onChangeState();
	}

	override hostDisconnected(): void {
		super.hostDisconnected();
		window.removeEventListener('changestate', this.onChangeState);
	}

	onChangeState = () => {
		const searchQuery = query();
		this.setFilterExpression(searchQuery.lq ?? '');

		let validLogLevels: LogLevelModel[] = [];
		if (searchQuery.loglevels) {
			const loglevels = searchQuery.loglevels.split(',') as LogLevelModel[];

			// Filter out invalid log levels that do not exist in LogLevelModel
			validLogLevels = loglevels.filter((loglevel) => {
				return Object.values(LogLevelModel).includes(loglevel);
			});
		}
		this.setLogLevelsFilter(validLogLevels);

		const dateRange: LogViewerDateRange = this.getDateRange() as LogViewerDateRange;

		this.setDateRange({
			startDate: searchQuery.startDate || dateRange.startDate,
			endDate: searchQuery.endDate || dateRange.endDate,
		});

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

		this.#dateRange.setValue({ startDate, endDate });
		this.validateLogSize();
		this.getLogCount();
		this.getMessageTemplates(0, 10);
	}

	getDateRange() {
		return this.#dateRange.getValue();
	}

	async getSavedSearches({ skip = 0, take = 999 }: { skip?: number; take?: number } = {}) {
		const { data } = await this.#repository.getSavedSearches({ skip, take });
		if (data) {
			this.#savedSearches.setValue(data);
		} else {
			//fallback to some default searches resembling Umbraco <= 13
			this.#savedSearches.setValue({
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

	async saveSearch({ name, query }: SavedLogSearchResponseModel) {
		const previousSavedSearches = this.#savedSearches.getValue()?.items ?? [];
		try {
			this.#savedSearches.update({ items: [...previousSavedSearches, { name, query }] });
			await this.#repository.saveSearch({ name, query });
		} catch {
			this.#savedSearches.update({ items: previousSavedSearches });
		}
	}

	async removeSearch({ name }: { name: string }) {
		const previousSavedSearches = this.#savedSearches.getValue()?.items ?? [];
		try {
			this.#savedSearches.update({ items: previousSavedSearches.filter((search) => search.name !== name) });
			await this.#repository.removeSearch({ name });
		} catch {
			this.#savedSearches.update({ items: previousSavedSearches });
		}
	}

	async getLogCount() {
		const { data } = await this.#repository.getLogCount({ ...this.#dateRange.getValue() });

		if (data) {
			this.#logCount.setValue(data);
		}
	}

	async getMessageTemplates(skip: number, take: number) {
		const { data } = await this.#repository.getMessageTemplates({ skip, take, ...this.#dateRange.getValue() });

		if (data) {
			this.#messageTemplates.setValue(data);
		}
	}

	async getLogLevels(skip: number, take: number) {
		const { data } = await this.#repository.getLogLevels({ skip, take });

		if (data) {
			this.#loggers.setValue(data);
		}
	}

	async validateLogSize() {
		const { error } = await this.#repository.getLogViewerValidateLogsSize({ ...this.#dateRange.getValue() });
		if (error) {
			this.#canShowLogs.setValue(false);
			return;
		}
		this.#canShowLogs.setValue(true);
	}

	setCurrentPage(page: number) {
		this.currentPage = page;
	}

	getLogs = async () => {
		if (this.#canShowLogs.getValue() === false) {
			return;
		}

		const isPollingEnabled = this.#polling.getValue().enabled;

		if (!isPollingEnabled) this.#isLoadingLogs.setValue(true);

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
		this.#isLoadingLogs.setValue(false);
		if (data) {
			this.#logs.setValue(data);
		}
	};

	setFilterExpression(query: string) {
		this.#filterExpression.setValue(query);
	}

	setLogLevelsFilter(logLevels: LogLevelModel[]) {
		this.#logLevelsFilter.setValue(logLevels);
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
		this.#sortingDirection.setValue(newDirection);
	}
}

export { UmbLogViewerWorkspaceContext as api };
