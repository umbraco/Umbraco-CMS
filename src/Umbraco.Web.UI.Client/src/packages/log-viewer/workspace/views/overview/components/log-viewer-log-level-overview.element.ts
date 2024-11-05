import type { UmbLogViewerWorkspaceContext } from '../../../logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { LoggerResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

@customElement('umb-log-viewer-log-level-overview')
export class UmbLogViewerLogLevelOverviewElement extends UmbLitElement {
	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getSavedSearches();
			this.#observeLogLevels();
		});
	}

	@state()
	private _loggers: LoggerResponseModel[] = [];
	/**
	 * The name of the logger to get the level for. Defaults to 'Global'.
	 * @memberof UmbLogViewerLogLevelOverviewElement
	 */
	@property()
	loggerName = 'Global';

	#observeLogLevels() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.loggers, (loggers) => {
			this._loggers = loggers ?? [];
		});
	}

	override render() {
		return html`${this._loggers.length > 0
			? this._loggers.find((logger) => logger.name === this.loggerName)?.level
			: nothing}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-log-level-overview': UmbLogViewerLogLevelOverviewElement;
	}
}
