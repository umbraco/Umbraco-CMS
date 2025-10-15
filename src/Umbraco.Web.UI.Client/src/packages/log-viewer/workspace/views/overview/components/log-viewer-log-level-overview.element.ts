import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { LoggerResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-log-level-overview')
export class UmbLogViewerLogLevelOverviewElement extends UmbLitElement {
	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE | undefined) {
		this.#logViewerContext = value;
		this.#logViewerContext?.getSavedSearches();
		this.#observeLogLevels();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
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
		this.observe(this._logViewerContext?.loggers, (loggers) => {
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
