import type { UmbLogViewerWorkspaceContext } from '../../../logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UUIPaginationEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-log-viewer-saved-searches-overview')
export class UmbLogViewerSavedSearchesOverviewElement extends UmbLitElement {
	#itemsPerPage = 999;
	#currentPage = 1;

	@state()
	private _savedSearches: SavedLogSearchResponseModel[] = [];

	@state()
	private _total = 0;

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getSavedSearches({ skip: 0, take: this.#itemsPerPage });
			this.#observeStuff();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches?.items ?? [];
			this._total = savedSearches?.total ?? 0;
		});
	}

	#getSavedSearches() {
		const skip = this.#currentPage * this.#itemsPerPage - this.#itemsPerPage;
		this.#logViewerContext?.getSavedSearches({ skip, take: this.#itemsPerPage });
	}

	#onChangePage(event: UUIPaginationEvent) {
		this.#currentPage = event.target.current;
		this.#getSavedSearches();
	}

	#renderSearchItem = (searchListItem: SavedLogSearchResponseModel) => {
		return html` <li>
			<uui-menu-item
				label="${searchListItem.name ?? ''}"
				title="${searchListItem.name ?? ''}"
				href=${`section/settings/workspace/logviewer/view/search/?lq=${encodeURIComponent(
					searchListItem.query ?? '',
				)}`}>
				<uui-icon slot="icon" name="icon-search"></uui-icon>${searchListItem.name}
			</uui-menu-item>
		</li>`;
	};

	override render() {
		return html` <uui-box id="saved-searches" headline="Saved searches">
			<ul>
				${this.#renderSearchItem({ name: 'All logs', query: '' })} ${this._savedSearches.map(this.#renderSearchItem)}
			</ul>
			${this._total > this.#itemsPerPage
				? html`<uui-pagination
						.current=${this.#currentPage}
						.total=${Math.ceil(this._total / this.#itemsPerPage)}
						firstlabel=${this.localize.term('general_first')}
                        previouslabel=${this.localize.term('general_previous')}
                        nextlabel=${this.localize.term('general_next')}
                        lastlabel=${this.localize.term('general_last')}
						@change=${this.#onChangePage}></uui-pagination>`
				: nothing}
		</uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
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
