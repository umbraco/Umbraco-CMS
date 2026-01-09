import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import type { UUICheckboxElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, queryAll, state } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { path, query, toQueryString } from '@umbraco-cms/backoffice/router';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-log-level-filter-menu')
export class UmbLogViewerLogLevelFilterMenuElement extends UmbLitElement {
	@queryAll('#log-level-selector > uui-checkbox')
	private _logLevelSelectorCheckboxes!: NodeListOf<UUICheckboxElement>;

	@state()
	private _logLevelFilter: LogLevelModel[] = [];

	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value) {
		this.#logViewerContext = value;
		this.#observeLogLevelFilter();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	#observeLogLevelFilter() {
		this.observe(this._logViewerContext?.logLevelsFilter, (levelsFilter) => {
			this._logLevelFilter = levelsFilter ?? [];
		});
	}

	#setLogLevel() {
		const logLevels = Array.from(this._logLevelSelectorCheckboxes)
			.filter((checkbox) => checkbox.checked)
			.map((checkbox) => checkbox.value as LogLevelModel);

		let q = query();

		if (logLevels.length) {
			q = { ...q, loglevels: logLevels.join(',') };
		} else {
			delete q.loglevels;
		}

		window.history.pushState({}, '', `${path()}?${toQueryString(q)}`);
	}

	setLogLevelDebounce = debounce(this.#setLogLevel, 300);

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
			<div id="log-level-selector" @change=${this.setLogLevelDebounce}>
				${Object.values(LogLevelModel).map(
					(logLevel) =>
						html`<uui-checkbox
							class="log-level-menu-item"
							.checked=${this._logLevelFilter.includes(logLevel)}
							.value=${logLevel}
							label="${logLevel}">
							<umb-log-viewer-level-tag .level=${logLevel}></umb-log-viewer-level-tag>
						</uui-checkbox>`,
				)}
				<uui-button
					class="log-level-menu-item"
					@click=${this.#selectAllLogLevels}
					label=${this.localize.term('logViewer_selectAllLogLevelFilters')}>
					<umb-localize key="logViewer_selectAllLogLevelFilters">Select all</umb-localize>
				</uui-button>
				<uui-button
					class="log-level-menu-item"
					@click=${this.#deselectAllLogLevels}
					label=${this.localize.term('logViewer_deselectAllLogLevelFilters')}>
					<umb-localize key="logViewer_deselectAllLogLevelFilters">Deselect all</umb-localize>
				</uui-button>
			</div>
		`;
	}

	override render() {
		return html`
			<umb-dropdown label=${this.localize.term('logViewer_selectLogLevels')}>
				<span slot="label">
					<umb-localize key="logViewer_logLevels">Log Levels</umb-localize>:
					${this._logLevelFilter.length > 0
						? this._logLevelFilter.map((level) => html`<span class="log-level-button-indicator">${level}</span>`)
						: html`<umb-localize key="logViewer_all">All</umb-localize>`}
				</span>
				${this.#renderLogLevelSelector()}
			</umb-dropdown>
		`;
	}

	static override styles = [
		css`
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-level-filter-menu': UmbLogViewerLogLevelFilterMenuElement;
	}
}
