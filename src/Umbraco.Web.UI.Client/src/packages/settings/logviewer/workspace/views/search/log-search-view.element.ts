import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../logviewer.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-log-viewer-search-view')
export class UmbLogViewerSearchViewElement extends UmbLitElement {
	@state()
	private _canShowLogs = true;

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	#canShowLogsObserver?: UmbObserverController<boolean | null>;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeCanShowLogs();
		});
	}

	#observeCanShowLogs() {
		if (this.#canShowLogsObserver) this.#canShowLogsObserver.destroy();
		if (!this.#logViewerContext) return;

		this.#canShowLogsObserver = this.observe(this.#logViewerContext.canShowLogs, (canShowLogs) => {
			this._canShowLogs = canShowLogs ?? this._canShowLogs;
		});
	}

	render() {
		return html`
			<umb-body-layout header-transparent header-height-fit>
				<div id="header" slot="header">
					<div id="levels-container">
						<umb-log-viewer-log-level-filter-menu></umb-log-viewer-log-level-filter-menu>
						<div id="dates-polling-container">
							<umb-log-viewer-date-range-selector horizontal></umb-log-viewer-date-range-selector>
							<umb-log-viewer-polling-button> </umb-log-viewer-polling-button>
						</div>
					</div>
					<div id="input-container">
						<umb-log-viewer-search-input></umb-log-viewer-search-input>
					</div>
				</div>

				<uui-box>
					${this._canShowLogs
						? html`<umb-log-viewer-messages-list></umb-log-viewer-messages-list>`
						: html`<umb-log-viewer-to-many-logs-warning
								id="to-many-logs-warning"></umb-log-viewer-to-many-logs-warning>`}
				</uui-box>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-box {
				--uui-box-default-padding: 0;
			}

			#header {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				width: 100%;
				padding: var(--uui-size-layout-1);
				padding-bottom: var(--uui-size-layout-1);
			}

			#levels-container,
			#input-container {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-4);
				width: 100%;
			}

			#levels-container {
				justify-content: space-between;
			}

			#dates-polling-container {
				display: flex;
				align-items: baseline;
			}

			umb-log-viewer-search-input {
				flex: 1;
			}

			umb-log-viewer-date-range-selector {
				flex-direction: row;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-view': UmbLogViewerSearchViewElement;
	}
}
