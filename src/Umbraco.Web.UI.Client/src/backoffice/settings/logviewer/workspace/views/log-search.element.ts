import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state, query, queryAll } from 'lit/decorators.js';
import {
	LogViewerDateRange,
	PoolingCOnfig,
	PoolingInterval,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { LogLevelModel, LogMessageModel, SavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UUICheckboxElement, UUIInputElement, UUIPopoverElement, UUISymbolExpandElement } from '@umbraco-ui/uui';

@customElement('umb-log-viewer-search-view')
export class UmbLogViewerSearchViewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#layout {
				margin: 20px;
			}
			#levels-container,
			#input-container {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-4);
				width: 100%;
				margin-bottom: 20px;
			}

			#levels-container {
				justify-content: space-between;
			}

			#input-container {
				justify-content: space-between;
			}

			#search-input {
				width: 100%;
			}

			#saved-searches-button {
				flex-shrink: 0;
			}

			#saved-searches-popover {
				flex: 1;
			}

			#saved-searches-container {
				width: 100%;
				max-height: 300px;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-1);
			}

			.saved-search-item {
				display: flex;
				justify-content: space-between;
				align-items: stretch;
				border-bottom: 1px solid #e9e9eb;
			}

			.saved-search-item-button {
				display: flex;
				font-family: inherit;
				flex: 1;
				background: 0 0;
				padding: 0 0;
				border: 0;
				clear: both;
				cursor: pointer;
				display: flex;
				font-weight: 400;
				line-height: 20px;
				text-align: left;
				align-items: center;
				white-space: nowrap;
				color: var(--uui-color-interactive);
			}

			.saved-search-item-button:hover {
				background-color: var(--uui-color-surface-emphasis, rgb(250, 250, 250));
				color: var(--color-standalone);
			}

			.saved-search-item-name {
				font-weight: 600;
				margin: 0 var(--uui-size-space-3);
			}

			#polling-symbol-expand,
			#saved-search-expand-symbol,
			uui-symbol-sort {
				margin-left: var(--uui-size-space-3);
			}

			#message-list-header {
				display: flex;
				font-weight: 600;
			}

			#message-list-header > div {
				box-sizing: border-box;
				padding: 10px 20px;
				display: flex;
				align-items: center;
			}

			#timestamp {
				flex: 1 0 14ch;
			}

			#level,
			#machine {
				flex: 1 0 14ch;
			}

			#message {
				flex: 6 0 14ch;
			}

			#log-level-selector {
				padding: var(--uui-box-default-padding, var(--uui-size-space-5, 18px));
				width: 150px;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-3);
			}

			.log-level-button-indicator {
				font-weight: 600;
			}

			.log-level-button-indicator:not(:last-of-type)::after {
				content: ', ';
			}

			#polling-interval-menu {
				margin: 0;
				padding: 0;
				width: 20ch;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				display: flex;
				flex-direction: column;
				transform: translateX(calc((100% - 33px) * -1));
			}
		`,
	];

	@query('#saved-searches-popover')
	private _savedSearchesPopover!: UUIPopoverElement;

	@query('#polling-popover')
	private _pollingPopover!: UUIPopoverElement;

	@query('#polling-expand-symbol')
	private _polingExpandSymbol!: UUISymbolExpandElement;

	@query('#saved-search-expand-symbol')
	private _savedSearchesExpandSymbol!: UUISymbolExpandElement;

	@queryAll('#log-level-selector > uui-checkbox')
	private _logLevelSelectorCheckboxes!: NodeListOf<UUICheckboxElement>;

	@state()
	private _savedSearches: SavedLogSearchModel[] = [];

	@state()
	private _startDate = '';

	@state()
	private _endDate = '';

	@state()
	private _inputQuery = '';

	@state()
	private _logs: LogMessageModel[] = [];

	@state()
	private _logLevel: LogLevelModel[] = [];

	@state()
	private _poolingConfig: PoolingCOnfig = {pooling: false, interval: 0};

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
			this.#logViewerContext.getLogs();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches ?? [];
		});

		this.observe(this.#logViewerContext.dateRange, (dateRange: LogViewerDateRange) => {
			this._startDate = dateRange?.startDate;
			this._endDate = dateRange?.endDate;
		});

		this.observe(this.#logViewerContext.filterExpression, (query) => {
			this._inputQuery = query;
		});

		this.observe(this.#logViewerContext.logs, (logs) => {
			this._logs = logs ?? [];
		});

		this.observe(this.#logViewerContext.logLevel, (levels) => {
			this._logLevel = levels ?? [];
		});

		
		this.observe(this.#logViewerContext.polling, (poolingConfig) => {
			this._poolingConfig = {...poolingConfig};
		});
	}

	#toggleSavedSearchesPopover() {
		this._savedSearchesPopover.open = !this._savedSearchesPopover.open;
	}

	#toggleSavedSearchesExpandSymbol() {
		this._savedSearchesExpandSymbol.open = !this._savedSearchesExpandSymbol.open;
	}

	#openSavedSearchesPopover() {
		this.#toggleSavedSearchesPopover();
		this.#toggleSavedSearchesExpandSymbol();
	}

	#setQuery(event: Event) {
		const target = event.target as UUIInputElement;
		this._inputQuery = target.value as string;
		this.#logViewerContext?.setFilterExpression(this._inputQuery);
	}

	#setQueryFromSavedSearch(query: string) {
		this._inputQuery = query;
		this.#logViewerContext?.setFilterExpression(query);
		this.#logViewerContext?.getLogs();
		this._savedSearchesPopover.open = false;
	}

	#clearQuery() {
		this._inputQuery = '';
		this.#logViewerContext?.setFilterExpression('');
		this.#logViewerContext?.getLogs();
	}

	#renderSearchInput() {
		return html`<uui-popover
				placement="bottom-start"
				id="saved-searches-popover"
				@close=${this.#toggleSavedSearchesExpandSymbol}>
				<uui-input
					id="search-input"
					label="Search logs"
					.placeholder=${'Search logs...'}
					slot="trigger"
					@input=${this.#setQuery}
					.value=${this._inputQuery}>
					${this._inputQuery
						? html`<uui-button compact slot="append" label="Save search"
									><uui-icon name="umb:favorite"></uui-icon></uui-button
								><uui-button compact slot="append" label="Clear" @click=${this.#clearQuery}
									><uui-icon name="umb:delete"></uui-icon
								></uui-button>`
						: html``}
					<uui-button
						compact
						slot="append"
						id="saved-searches-button"
						@click=${this.#openSavedSearchesPopover}
						label="Saved searches"
						>Saved searches <uui-symbol-expand id="saved-search-expand-symbol"></uui-symbol-expand
					></uui-button>
				</uui-input>

				<uui-scroll-container slot="popover" id="saved-searches-container" role="list">
					${this._savedSearches.map(
						(search) =>
							html`<li class="saved-search-item">
								<button
									label="Search for ${search.name}"
									class="saved-search-item-button"
									@click=${() => this.#setQueryFromSavedSearch(search.query ?? '')}>
									<span class="saved-search-item-name">${search.name}</span>
									<span class="saved-search-item-query">${search.query}</span></button
								><uui-button label="Remove saved search" color="danger"
									><uui-icon name="umb:trash"></uui-icon
								></uui-button>
							</li>`
					)}
				</uui-scroll-container>
			</uui-popover>
			<uui-button look="primary">Search</uui-button>`;
	}

	#setLogLevel() {
		const logLevels = Array.from(this._logLevelSelectorCheckboxes)
			.filter((checkbox) => checkbox.checked)
			.map((checkbox) => checkbox.value as LogLevelModel);
		this.#logViewerContext?.setLogLevels(logLevels);
		this.#logViewerContext?.getLogs();
	}

	#selectAllLogLevels() {
		this._logLevelSelectorCheckboxes.forEach((checkbox) => (checkbox.checked = true));
		this.#setLogLevel();
	}

	#deselectAllLogLevels() {
		this._logLevelSelectorCheckboxes.forEach((checkbox) => (checkbox.checked = false));
		this.#setLogLevel();
	}

	#renderLogLevelSelector() {
		return html`
			<div slot="dropdown" id="log-level-selector" @change=${this.#setLogLevel}>
				${Object.values(LogLevelModel).map(
					(logLevel) =>
						html`<uui-checkbox class="log-level-menu-item" .value=${logLevel} label="${logLevel}"
							><umb-log-viewer-level-tag .level=${logLevel}></umb-log-viewer-level-tag
						></uui-checkbox>`
				)}
				<uui-button class="log-level-menu-item" @click=${this.#selectAllLogLevels} label="Select all"
					>Select all</uui-button
				>
				<uui-button class="log-level-menu-item" @click=${this.#deselectAllLogLevels} label="Deselect all"
					>Deselect all</uui-button
				>
			</div>
		`;
	}

	#togglePolling() {
		this.#logViewerContext?.togglePolling();
	}

	#setPolingInterval(interval: PoolingInterval) {
		this.#logViewerContext?.setPollingInterval(interval);
		this.#closePoolingPopover();
	}

	#openPoolingPopover() {
		this._pollingPopover.open = true;
		this._polingExpandSymbol.open = true;
	}

	#closePoolingPopover() {
		this._pollingPopover.open = false;
		this._polingExpandSymbol.open = false;
	}

	#pollingIntervals: PoolingInterval[] = [2000, 5000, 10000, 20000, 30000];

	#renderPolingTimeSelector() {
		return html` <uui-button-group>
			<uui-button label="Start pooling" @click=${this.#togglePolling}>${this._poolingConfig.enabled ? html`Polling ${this._poolingConfig.interval/1000} seconds` : 'Pooling'}</uui-button>
			<uui-popover placement="bottom-end" id="polling-popover" @close=${() => (this._polingExpandSymbol.open = false)}>
				<uui-button slot="trigger" compact label="Choose pooling time" @click=${this.#openPoolingPopover}>
					<uui-symbol-expand id="polling-expand-symbol"></uui-symbol-expand>
				</uui-button>

				<ul id="polling-interval-menu" slot="popover">
					${this.#pollingIntervals.map(
						(interval: PoolingInterval) =>
							html`<uui-menu-item
								label="Every ${interval/1000} seconds"
								@click-label=${() => this.#setPolingInterval(interval)}></uui-menu-item>`
					)}
				</ul>
			</uui-popover>
		</uui-button-group>`;
	}

	render() {
		return html`
			<div id="layout">
				<div id="levels-container">
					<umb-button-with-dropdown label="Select log levels"
						>Log Level:
						${this._logLevel.length > 0
							? this._logLevel.map((level) => html`<span class="log-level-button-indicator">${level}</span>`)
							: 'All'}
						${this.#renderLogLevelSelector()}
					</umb-button-with-dropdown>
					${this.#renderPolingTimeSelector()}
				</div>
				<div id="input-container">${this.#renderSearchInput()}</div>
				<uui-box>
					<p>Total items: ${this._logs.length}</p>
					<div id="message-list-header">
						<div id="timestamp">Timestamp <uui-symbol-sort active></uui-symbol-sort></div>
						<div id="level">Level</div>
						<div id="machine">Machine name</div>
						<div id="message">Message</div>
					</div>
					${this._logs.map(
						(log) => html`<umb-log-viewer-message
							.timestamp=${log.timestamp ?? ''}
							.level=${log.level ?? ''}
							.renderedMessage=${log.renderedMessage ?? ''}
							.properties=${log.properties ?? []}
							.exception=${log.exception ?? ''}
							.messageTemplate=${log.messageTemplate ?? ''}></umb-log-viewer-message>`
					)}
				</uui-box>
			</div>
		`;
	}
}

export default UmbLogViewerSearchViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-view': UmbLogViewerSearchViewElement;
	}
}
