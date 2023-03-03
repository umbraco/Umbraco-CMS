import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state, query } from 'lit/decorators.js';
import {
	UUIInputElement,
	UUIPaginationElement,
	UUIPopoverElement,
	UUIScrollContainerElement,
	UUISymbolExpandElement,
} from '@umbraco-ui/uui';
import {
	LogViewerDateRange,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../logviewer.context';
import { DirectionModel, LogMessageModel, SavedLogSearchModel } from '@umbraco-cms/backend-api';
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

			umb-log-viewer-search-input {
				flex: 1;
			}
		`,
	];

	render() {
		return html`
			<div id="layout">
				<div id="levels-container">
					<umb-log-viewer-log-level-filter-menu></umb-log-viewer-log-level-filter-menu>
					<umb-log-viewer-polling-button> </umb-log-viewer-polling-button>
				</div>
				<div id="input-container">
					<umb-log-viewer-search-input></umb-log-viewer-search-input>
				</div>
				<umb-log-viewer-messages-list></umb-log-viewer-messages-list>
			</div>
		`;
	}
}

export default UmbLogViewerSearchViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-view': UmbLogViewerSearchViewElement;
	}
}
