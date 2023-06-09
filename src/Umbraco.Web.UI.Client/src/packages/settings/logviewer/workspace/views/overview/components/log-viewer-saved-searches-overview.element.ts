import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/backend-api';

//TODO: implement the saved searches pagination when the API total bug is fixed
@customElement('umb-log-viewer-saved-searches-overview')
export class UmbLogViewerSavedSearchesOverviewElement extends UmbLitElement {
	@state()
	private _savedSearches: SavedLogSearchResponseModel[] = [];

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getSavedSearches();
			this.#observeStuff();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches ?? [];
		});
	}

	#renderSearchItem = (searchListItem: SavedLogSearchResponseModel) => {
		return html` <li>
			<uui-menu-item
				label="${searchListItem.name ?? ''}"
				title="${searchListItem.name ?? ''}"
				href=${`section/settings/workspace/logviewer/search/?lq=${encodeURIComponent(searchListItem.query ?? '')}`}>
				<uui-icon slot="icon" name="umb:search"></uui-icon>${searchListItem.name}
			</uui-menu-item>
		</li>`;
	};

	render() {
		return html` <uui-box id="saved-searches" headline="Saved searches">
			<ul>
				${this.#renderSearchItem({ name: 'All logs', query: '' })} ${this._savedSearches.map(this.#renderSearchItem)}
			</ul>
		</uui-box>`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-box {
				height: 100%;
			}

			ul {
				list-style: none;
				margin: 0;
				padding: 0;
				border: 0;
				font-size: 100%;
				font: inherit;
				vertical-align: baseline;
			}

			li {
				display: flex;
				align-items: center;
			}

			li uui-icon {
				margin-right: 1em;
			}

			uui-menu-item {
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-saved-searches-overview': UmbLogViewerSavedSearchesOverviewElement;
	}
}
