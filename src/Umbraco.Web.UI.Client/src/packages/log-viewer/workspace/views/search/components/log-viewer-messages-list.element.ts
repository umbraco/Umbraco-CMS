import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import type { UUIScrollContainerElement, UUIPaginationElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { LogMessageResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';
import { skip } from '@umbraco-cms/backoffice/external/rxjs';

@customElement('umb-log-viewer-messages-list')
export class UmbLogViewerMessagesListElement extends UmbLitElement {
	@query('#logs-scroll-container')
	private _logsScrollContainer!: UUIScrollContainerElement;

	@state()
	private _sortingDirection: DirectionModel = DirectionModel.ASCENDING;

	@state()
	private _logs: LogMessageResponseModel[] = [];

	@state()
	private _logsTotal = 0;

	@state()
	private _isLoading = true;

	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value) {
		this.#logViewerContext = value;
		this.#observeLogs();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	#observeLogs() {
		this.observe(this._logViewerContext?.logs, (logs) => {
			this._logs = logs ?? [];
		});

		this.observe(this._logViewerContext?.isLoadingLogs, (isLoading) => {
			this._isLoading = isLoading ?? this._isLoading;
		});

		this.observe(this._logViewerContext?.logsTotal, (total) => {
			this._logsTotal = total ?? 0;
		});

		this.observe(this._logViewerContext?.sortingDirection, (direction) => {
			this._sortingDirection = direction ?? this._sortingDirection;
		});

		// Observe filter expression changes to trigger search
		// Only observes when this component is mounted (when logs are visible)
		this.observe(
			this._logViewerContext?.filterExpression.pipe(
				skip(1), // Skip initial value to avoid duplicate search on page load
			),
			() => {
				this._logViewerContext?.getLogs();
			},
		);
	}

	#sortLogs() {
		this._logViewerContext?.toggleSortOrder();
		this._logViewerContext?.setCurrentPage(1);
		this._logViewerContext?.getLogs();
	}

	#onPageChange(event: Event): void {
		const current = (event.target as UUIPaginationElement).current;
		this._logViewerContext?.setCurrentPage(current);
		this._logViewerContext?.getLogs();
		this._logsScrollContainer.scrollTop = 0;
	}

	private _renderPagination() {
		if (!this._logsTotal) return '';

		const totalPages = Math.ceil(this._logsTotal / 100);

		if (totalPages <= 1) return '';

		return html`<div id="pagination">
			<uui-pagination
				.total=${totalPages}
				firstlabel=${this.localize.term('general_first')}
				previouslabel=${this.localize.term('general_previous')}
				nextlabel=${this.localize.term('general_next')}
				lastlabel=${this.localize.term('general_last')}
				@change="${this.#onPageChange}"></uui-pagination>
		</div>`;
	}

	#renderLogs() {
		return html`${this._logs.length > 0
			? html` ${this._logs.map(
					(log) =>
						html`<umb-log-viewer-message
							.timestamp=${log.timestamp ?? ''}
							.level=${log.level ?? ''}
							.renderedMessage=${log.renderedMessage ?? ''}
							.properties=${log.properties ?? []}
							.exception=${log.exception ?? ''}
							.messageTemplate=${log.messageTemplate ?? ''}></umb-log-viewer-message>`,
				)}`
			: html`
					<span id="empty">
						<uui-icon name="icon-search"></uui-icon
						><umb-localize key="logViewer_noResults">Sorry, we cannot find what you are looking for.</umb-localize>
					</span>
				`}`;
	}

	override render() {
		// TODO: the table should scroll instead of the whole main div
		return html`<uui-box>
				<div id="header" slot="header">
					<div id="timestamp">
						<umb-localize key="logViewer_timestamp">Timestamp</umb-localize>
						<uui-button compact @click=${this.#sortLogs} label=${this.localize.term('logViewer_sortLogs')}>
							<uui-symbol-sort
								?descending=${this._sortingDirection === DirectionModel.DESCENDING}
								active></uui-symbol-sort>
						</uui-button>
					</div>
					<div id="level"><umb-localize key="logViewer_level">Level</umb-localize></div>
					<div id="machine"><umb-localize key="logViewer_machine">Machine name</umb-localize></div>
					<div id="message"><umb-localize key="logViewer_message">Message</umb-localize></div>
				</div>
				<div id="main">
					${this._isLoading
						? html`
								<span id="empty">
									<uui-loader-circle></uui-loader-circle
									><umb-localize key="logViewer_loadingLogs">Loading log messages...</umb-localize>
								</span>
							`
						: html`${this.#renderLogs()}`}
				</div>
			</uui-box>
			${this._renderPagination()} `;
	}

	static override styles = [
		css`
			uui-pagination {
				display: block;
				margin-bottom: var(--uui-size-layout-1);
			}
			uui-box {
				--uui-box-default-padding: 0;
			}

			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
			}
			#header {
				display: flex;
				font-weight: 600;
				width: 100%;
			}

			#header > div {
				box-sizing: border-box;
				padding: 10px 20px;
				display: flex;
				align-items: center;
			}

			#main {
				display: flex;
				flex-direction: column;
				width: 100%;
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
				margin: var(--uui-size-space-5) 0;
			}

			#pagination {
				display: block;
				margin: var(--uui-size-space-5) 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-messages-list': UmbLogViewerMessagesListElement;
	}
}
