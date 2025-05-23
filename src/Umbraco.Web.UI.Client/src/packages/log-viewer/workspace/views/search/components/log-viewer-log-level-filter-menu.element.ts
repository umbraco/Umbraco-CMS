import type { UmbLogViewerWorkspaceContext } from '../../../logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import type { UUICheckboxElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, queryAll, state } from '@umbraco-cms/backoffice/external/lit';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { path, query, toQueryString } from '@umbraco-cms/backoffice/router';

@customElement('umb-log-viewer-log-level-filter-menu')
export class UmbLogViewerLogLevelFilterMenuElement extends UmbLitElement {
	@queryAll('#log-level-selector > uui-checkbox')
	private _logLevelSelectorCheckboxes!: NodeListOf<UUICheckboxElement>;

	@state()
	private _logLevelFilter: LogLevelModel[] = [];

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#observeLogLevelFilter();
		});
	}

	#observeLogLevelFilter() {
		if (!this.#logViewerContext) return;

		this.observe(this.#logViewerContext.logLevelsFilter, (levelsFilter) => {
			this._logLevelFilter = levelsFilter ?? [];
		});
	}

	#setLogLevel() {
		if (!this.#logViewerContext) return;

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
					label=${this.localize.term('general_selectAll')}></uui-button>
				<uui-button class="log-level-menu-item" @click=${this.#deselectAllLogLevels} label="Deselect all"
					>Deselect all</uui-button
				>
			</div>
		`;
	}

	override render() {
		return html`
			<umb-dropdown label="Select log levels">
				<span slot="label">
					Log Level:
					${this._logLevelFilter.length > 0
						? this._logLevelFilter.map((level) => html`<span class="log-level-button-indicator">${level}</span>`)
						: 'All'}
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
