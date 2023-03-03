import { UUICheckboxElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, queryAll, state } from 'lit/decorators.js';
import _ from 'lodash';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context';
import { LogLevelModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-log-viewer-log-level-filter-menu')
export class UmbLogViewerLogLevelFilterMenuElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
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

	@queryAll('#log-level-selector > uui-checkbox')
	private _logLevelSelectorCheckboxes!: NodeListOf<UUICheckboxElement>;

	@state()
	private _logLevelFilter: LogLevelModel[] = [];

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
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
		this.#logViewerContext?.setCurrentPage(1);

		const logLevels = Array.from(this._logLevelSelectorCheckboxes)
			.filter((checkbox) => checkbox.checked)
			.map((checkbox) => checkbox.value as LogLevelModel);
		this.#logViewerContext?.setLogLevelsFilter(logLevels);
		this.#logViewerContext.getLogs();
	}

	setLogLevelDebounce = _.debounce(this.#setLogLevel, 300);

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
			<div slot="dropdown" id="log-level-selector" @change=${this.setLogLevelDebounce}>
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

	render() {
		return html`
			<umb-button-with-dropdown label="Select log levels"
				>Log Level:
				${this._logLevelFilter.length > 0
					? this._logLevelFilter.map((level) => html`<span class="log-level-button-indicator">${level}</span>`)
					: 'All'}
				${this.#renderLogLevelSelector()}
			</umb-button-with-dropdown>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-level-filter-menu': UmbLogViewerLogLevelFilterMenuElement;
	}
}
