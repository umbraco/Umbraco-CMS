import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state, query } from 'lit/decorators.js';
import {
	UUIInputElement,
	UUIPaginationElement,
	UUIPopoverElement,
	UUIScrollContainerElement,
	UUISymbolExpandElement,
} from '@umbraco-ui/uui';
import {
	LogViewerDateRange,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../logviewer.context';
import { DirectionModel, LogMessageModel, SavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

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

			umb-log-viewer-search-input {
				flex: 1;
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

			#empty {
				display: flex;
				justify-content: center;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			#pagination {
				margin: var(--uui-size-space-5, 18px) 0;
			}
		`,
	];

	@query('#logs-scroll-container')
	private _logsScrollContainer!: UUIScrollContainerElement;

	@state()
	private _sortingDirection: DirectionModel = DirectionModel.ASCENDING;

	@state()
	private _logs: LogMessageModel[] = [];

	@state()
	private _logsTotal = 0;

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

		this.observe(this.#logViewerContext.logs, (logs) => {
			this._logs = logs ?? [];
		});

		this.observe(this.#logViewerContext.logsTotal, (total) => {
			this._logsTotal = total ?? 0;
		});

		this.observe(this.#logViewerContext.sortingDirection, (direction) => {
			this._sortingDirection = direction;
		});
	}

	#renderSearchInput() {
		return html`<umb-log-viewer-search-input></umb-log-viewer-search-input>`;
	}

	#renderPolingTimeSelector() {
		return html` <umb-log-viewer-polling-button> </umb-log-viewer-polling-button>`;
	}

	#sortLogs() {
		this.#logViewerContext?.toggleSortOrder();
		this.#logViewerContext?.setCurrentPage(1);
		this.#logViewerContext?.getLogs();
	}

	render() {
		return html`
			<div id="layout">
				<div id="levels-container">
					<umb-log-viewer-log-level-filter-menu></umb-log-viewer-log-level-filter-menu>
					${this.#renderPolingTimeSelector()}
				</div>
				<div id="input-container">${this.#renderSearchInput()}</div>
				<uui-box>
					<p style="font-weight: bold;">Total items: ${this._logsTotal}</p>
					<div id="message-list-header">
						<div id="timestamp">
							Timestamp
							<uui-button compact @click=${this.#sortLogs}>
								<uui-symbol-sort
									?descending=${this._sortingDirection === DirectionModel.DESCENDING}
									active></uui-symbol-sort>
							</uui-button>
						</div>
						<div id="level">Level</div>
						<div id="machine">Machine name</div>
						<div id="message">Message</div>
					</div>
					<uui-scroll-container id="logs-scroll-container" style="max-height: calc(100vh - 490px)">
						${this._logs.length > 0
							? html` ${this._logs.map(
									(log) => html`<umb-log-viewer-message
										.timestamp=${log.timestamp ?? ''}
										.level=${log.level ?? ''}
										.renderedMessage=${log.renderedMessage ?? ''}
										.properties=${log.properties ?? []}
										.exception=${log.exception ?? ''}
										.messageTemplate=${log.messageTemplate ?? ''}></umb-log-viewer-message>`
							  )}`
							: html`<umb-empty-state size="small"
									><span id="empty">
										<uui-icon name="umb:search"></uui-icon>Sorry, we cannot find what you are looking for.
									</span></umb-empty-state
							  >`}
					</uui-scroll-container>
					${this._renderPagination()}
				</uui-box>
			</div>
		`;
	}

	_onPageChange(event: Event): void {
		const current = (event.target as UUIPaginationElement).current;
		this.#logViewerContext?.setCurrentPage(current);
		this.#logViewerContext?.getLogs();
		this._logsScrollContainer.scrollTop = 0;
	}

	private _renderPagination() {
		if (!this._logsTotal) return '';

		const totalPages = Math.ceil(this._logsTotal / 100);

		if (totalPages <= 1) return '';

		return html`<div id="pagination">
			<uui-pagination .total=${totalPages} @change="${this._onPageChange}"></uui-pagination>
		</div>`;
	}
}

export default UmbLogViewerSearchViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-view': UmbLogViewerSearchViewElement;
	}
}
