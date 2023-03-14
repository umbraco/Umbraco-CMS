import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../logviewer.context';
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

	@state()
	private _canShowLogs = false;

	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeCanShowLogs();
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
			<div id="layout">
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
				${this._canShowLogs
					? html`<umb-log-viewer-messages-list></umb-log-viewer-messages-list>`
					: html`<umb-log-viewer-to-many-logs-warning id="to-many-logs-warning"></umb-log-viewer-to-many-logs-warning>`}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-view': UmbLogViewerSearchViewElement;
	}
}
