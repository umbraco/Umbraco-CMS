import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../logviewer.context';
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

			#input-container {
				justify-content: space-between;
			}

			#search-input {
				flex: 1;
			}

			#saved-searches-button {
				flex-shrink: 0;
			}
		`,
	];

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
		});
	}

	render() {
		return html`
			<div id="layout">
				<div id="levels-container">
					<uui-button>Log level: All <uui-symbol-expand></uui-symbol-expand></uui-button>
					<uui-button-group>
						<uui-button>Polling</uui-button>
						<uui-button compact><uui-symbol-expand></uui-symbol-expand></uui-button>
					</uui-button-group>
				</div>
				<div id="input-container">
					<uui-input id="search-input" .placeholder=${'Search logs...'}>
						<uui-button slot="append" id="saved-searches-button"
							>Saved searches <uui-symbol-expand></uui-symbol-expand
						></uui-button>
					</uui-input>
					<uui-button look="primary">Search</uui-button>
				</div>
				<uui-box>
					<p>Total items: 234</p>
				</uui-box>
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
