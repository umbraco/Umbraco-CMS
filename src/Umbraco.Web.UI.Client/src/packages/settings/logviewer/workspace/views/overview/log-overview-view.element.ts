import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../logviewer.context.js';
import { LogLevelCountsReponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

//TODO: add a disabled attribute to the show more button when the total number of items is correctly returned from the endpoint
@customElement('umb-log-viewer-overview-view')
export class UmbLogViewerOverviewViewElement extends UmbLitElement {
	@state()
	private _errorCount = 0;

	@state()
	private _logLevelCount: LogLevelCountsReponseModel | null = null;

	@state()
	private _canShowLogs = false;

	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeErrorCount();
			this.#observeCanShowLogs();
			this.#logViewerContext?.getLogLevels(0, 100);
		});
	}

	#observeErrorCount() {
		if (!this.#logViewerContext) return;

		this.observe(this.#logViewerContext.logCount, (logLevelCount) => {
			this._errorCount = logLevelCount?.error ?? 0;
		});
	}

	#observeCanShowLogs() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.canShowLogs, (canShowLogs) => {
			this._canShowLogs = canShowLogs ?? false;
		});
	}

	render() {
		return html`
			<div id="logviewer-layout">
				<div id="info">
					<uui-box id="time-period" headline="Time Period">
						<umb-log-viewer-date-range-selector></umb-log-viewer-date-range-selector>
					</uui-box>

					<uui-box id="errors" headline="Number of Errors">
						<uui-button
							label="Show error logs"
							href=${`section/settings/workspace/logviewer/search/?lq=${encodeURIComponent(
								`@Level='Fatal' or @Level='Error' or Has(@Exception)`
							)}`}>
							<h2 id="error-count">${this._errorCount}</h2></uui-button
						>
					</uui-box>

					<uui-box id="level" headline="Log level">
						<h2 id="log-level"><umb-log-viewer-log-level-overview></umb-log-viewer-log-level-overview></h2>
					</uui-box>

					<umb-log-viewer-log-types-chart id="types"></umb-log-viewer-log-types-chart>
				</div>

				${this._canShowLogs
					? html`<div id="saved-searches-container">
								<umb-log-viewer-saved-searches-overview></umb-log-viewer-saved-searches-overview>
							</div>

							<div id="common-messages-container">
								<umb-log-viewer-message-templates-overview></umb-log-viewer-message-templates-overview>
							</div>`
					: html`<umb-log-viewer-to-many-logs-warning id="to-many-logs-warning"></umb-log-viewer-to-many-logs-warning>`}
			</div>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}

			#logviewer-layout {
				padding-bottom: var(--uui-size-layout-1);
				display: grid;
				grid-template-columns: 7fr 2fr;
				grid-template-rows: auto auto;
				gap: 20px 20px;
				grid-auto-flow: row;
				grid-template-areas:
					'saved-searches info'
					'common-messages info';
			}

			#info {
				grid-area: info;
				align-self: start;
				display: grid;
				grid-template-columns: repeat(2, 1fr);
				grid-template-rows: repeat(4, max-content);
				gap: 20px 20px;
			}

			#time-period {
				grid-area: 1 / 1 / 2 / 3;
			}

			#errors {
				grid-area: 2 / 1 / 3 / 2;
				--uui-box-default-padding: 0;
			}

			#errors > uui-button {
				width: 100%;
			}

			#level {
				grid-area: 2 / 2 / 3 / 3;
			}

			#log-level {
				color: var(--uui-color-positive);
				text-align: center;
				margin: 0;
			}

			#types {
				grid-area: 3 / 1 / 5 / 3;
			}

			#saved-searches-container,
			to-many-logs-warning {
				grid-area: saved-searches;
			}

			#common-messages-container {
				grid-area: common-messages;
				--uui-box-default-padding: 0 var(--uui-size-space-5, 18px) var(--uui-size-space-5, 18px)
					var(--uui-size-space-5, 18px);
			}

			#common-messages-container > uui-box {
				height: 100%;
			}

			uui-label:nth-of-type(2) {
				display: block;
				margin-top: var(--uui-size-space-5);
			}

			#error-count {
				font-size: 3rem;
				text-align: center;
				color: var(--uui-color-danger);
				margin: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-overview-view': UmbLogViewerOverviewViewElement;
	}
}
