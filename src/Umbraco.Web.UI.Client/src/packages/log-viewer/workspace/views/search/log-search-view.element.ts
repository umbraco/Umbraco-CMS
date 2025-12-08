import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../logviewer-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-search-view')
export class UmbLogViewerSearchViewElement extends UmbLitElement {
	@state()
	private _canShowLogs = true;

	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value) {
		this.#logViewerContext = value;
		this.#observeCanShowLogs();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	#canShowLogsObserver?: UmbObserverController<boolean | null>;

	#observeCanShowLogs() {
		if (this.#canShowLogsObserver) this.#canShowLogsObserver.destroy();

		this.#canShowLogsObserver = this.observe(this._logViewerContext?.canShowLogs, (canShowLogs) => {
			this._canShowLogs = canShowLogs ?? this._canShowLogs;
		});
	}

	override render() {
		return html`
			<umb-body-layout header-transparent header-fit-height>
				<div id="header" slot="header" role="search" aria-label="Filter logs">
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

				${this._canShowLogs
					? html`<umb-log-viewer-messages-list></umb-log-viewer-messages-list>`
					: html`<umb-log-viewer-to-many-logs-warning id="to-many-logs-warning"></umb-log-viewer-to-many-logs-warning>`}
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				margin-bottom: var(--uui-size-space-2);
			}

			uui-box {
				--uui-box-default-padding: 0;
			}

			#header {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				width: 100%;
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
