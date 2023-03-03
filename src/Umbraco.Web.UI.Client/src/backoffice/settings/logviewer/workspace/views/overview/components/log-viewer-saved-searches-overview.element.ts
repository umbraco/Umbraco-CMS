import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { SavedLogSearchModel } from '@umbraco-cms/backend-api';

//TODO: implement the saved searches pagination when the API total bug is fixed
@customElement('umb-log-viewer-saved-searches-overview')
export class UmbLogViewerSavedSearchesOverviewElement extends UmbLitElement {
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
		`,
	];

	@state()
	private _savedSearches: SavedLogSearchModel[] = [];

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

	#setCurrentQuery(query: string) {
		this.#logViewerContext?.setFilterExpression(query);
	}

	#renderSearchItem = (searchListItem: SavedLogSearchModel) => {
		return html` <li>
			<uui-button
				@click=${() => {
					this.#setCurrentQuery(searchListItem.query ?? '');
				}}
				label="${searchListItem.name ?? ''}"
				title="${searchListItem.name ?? ''}"
				href=${'/section/settings/logviewer/search?lq=' + searchListItem.query}
				><uui-icon name="umb:search"></uui-icon>${searchListItem.name}</uui-button
			>
		</li>`;
	};

	render() {
		return html` <uui-box id="saved-searches" headline="Saved searches">
			<ul>
				<li>
					<uui-button
						@click=${() => {
							this.#setCurrentQuery('');
						}}
						label="All logs"
						title="All logs"
						href="/section/settings/logviewer/search"
						><uui-icon name="umb:search"></uui-icon>All logs</uui-button
					>
				</li>
				${this._savedSearches.map(this.#renderSearchItem)}
			</ul>
		</uui-box>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-saved-searches-overview': UmbLogViewerSavedSearchesOverviewElement;
	}
}
