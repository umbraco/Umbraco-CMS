import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { LoggerResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { consumeContext, observedFrom } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-log-level-overview')
export class UmbLogViewerLogLevelOverviewElement extends UmbLitElement {
	@consumeContext({
		context: UMB_APP_LOG_VIEWER_CONTEXT,
		callback: (ctx) => ctx?.getSavedSearches(),
	})
	private _logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@observedFrom(UMB_APP_LOG_VIEWER_CONTEXT, (ctx) => ctx.loggers, { default: [] })
	@state()
	private _loggers: LoggerResponseModel[] = [];

	/**
	 * The name of the logger to get the level for. Defaults to 'Global'.
	 * @memberof UmbLogViewerLogLevelOverviewElement
	 */
	@property()
	loggerName = 'Global';

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
