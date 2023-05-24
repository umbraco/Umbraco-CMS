import { html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LoggerResponseModel } from '@umbraco-cms/backoffice/backend-api';

//TODO: implement the saved searches pagination when the API total bug is fixed
@customElement('umb-log-viewer-log-level-overview')
export class UmbLogViewerLogLevelOverviewElement extends UmbLitElement {
	#logViewerContext?: UmbLogViewerWorkspaceContext;
	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getSavedSearches();
			this.#observeLogLevels();
		});
	}

	@state()
	private _loggers: LoggerResponseModel[] = [];
	/**
	 * The name of the logger to get the level for. Defaults to 'Global'.
	 *
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

	render() {
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
