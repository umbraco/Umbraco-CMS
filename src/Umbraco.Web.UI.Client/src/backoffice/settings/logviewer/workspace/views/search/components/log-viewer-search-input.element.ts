import { UUIInputElement, UUIPopoverElement, UUISymbolExpandElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context';
import { SavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-log-viewer-search-input')
export class UmbLogViewerSearchInputElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				align-items: center;
				justify-content: space-between;
				gap: var(--uui-size-space-4);
			}

			#search-input {
				width: 100%;
			}

			#saved-searches-button {
				flex-shrink: 0;
			}

			#saved-searches-popover {
				flex: 1;
			}

			#saved-searches-container {
				width: 100%;
				max-height: 300px;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-1);
			}

			.saved-search-item {
				display: flex;
				justify-content: space-between;
				align-items: stretch;
				border-bottom: 1px solid #e9e9eb;
			}

			.saved-search-item-button {
				display: flex;
				font-family: inherit;
				flex: 1;
				background: 0 0;
				padding: 0 0;
				border: 0;
				clear: both;
				cursor: pointer;
				display: flex;
				font-weight: 400;
				line-height: 20px;
				text-align: left;
				align-items: center;
				white-space: nowrap;
				color: var(--uui-color-interactive);
			}

			.saved-search-item-button:hover {
				background-color: var(--uui-color-surface-emphasis, rgb(250, 250, 250));
				color: var(--color-standalone);
			}

			.saved-search-item-name {
				font-weight: 600;
				margin: 0 var(--uui-size-space-3);
			}

			#polling-symbol-expand,
			#saved-search-expand-symbol,
			uui-symbol-sort {
				margin-left: var(--uui-size-space-3);
			}
		`,
	];

	@query('#saved-searches-popover')
	private _savedSearchesPopover!: UUIPopoverElement;

	@query('#saved-search-expand-symbol')
	private _savedSearchesExpandSymbol!: UUISymbolExpandElement;

	@state()
	private _savedSearches: SavedLogSearchModel[] = [];

	@state()
	private _inputQuery = '';

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
			this.#logViewerContext.getLogs();
		});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches ?? [];
		});

		this.observe(this.#logViewerContext.filterExpression, (query) => {
			this._inputQuery = query;
		});
	}

	#toggleSavedSearchesPopover() {
		this._savedSearchesPopover.open = !this._savedSearchesPopover.open;
	}

	#toggleSavedSearchesExpandSymbol() {
		this._savedSearchesExpandSymbol.open = !this._savedSearchesExpandSymbol.open;
	}

	#openSavedSearchesPopover() {
		this.#toggleSavedSearchesPopover();
		this.#toggleSavedSearchesExpandSymbol();
	}

	#setQuery(event: Event) {
		const target = event.target as UUIInputElement;
		this._inputQuery = target.value as string;
		this.#logViewerContext?.setFilterExpression(this._inputQuery);
	}

	#setQueryFromSavedSearch(query: string) {
		this._inputQuery = query;
		this.#logViewerContext?.setFilterExpression(query);
		this.#logViewerContext?.setCurrentPage(1);

		this.#logViewerContext?.getLogs();
		this._savedSearchesPopover.open = false;
	}

	#clearQuery() {
		this._inputQuery = '';
		this.#logViewerContext?.setFilterExpression('');
		this.#logViewerContext?.getLogs();
	}

	#search() {
		this.#logViewerContext?.setCurrentPage(1);

		this.#logViewerContext?.getLogs();
	}

	render() {
		return html` <uui-popover
				placement="bottom-start"
				id="saved-searches-popover"
				@close=${this.#toggleSavedSearchesExpandSymbol}>
				<uui-input
					id="search-input"
					label="Search logs"
					.placeholder=${'Search logs...'}
					slot="trigger"
					@input=${this.#setQuery}
					.value=${this._inputQuery}>
					${this._inputQuery
						? html`<uui-button compact slot="append" label="Save search"
									><uui-icon name="umb:favorite"></uui-icon></uui-button
								><uui-button compact slot="append" label="Clear" @click=${this.#clearQuery}
									><uui-icon name="umb:delete"></uui-icon
								></uui-button>`
						: html``}
					<uui-button
						compact
						slot="append"
						id="saved-searches-button"
						@click=${this.#openSavedSearchesPopover}
						label="Saved searches"
						>Saved searches <uui-symbol-expand id="saved-search-expand-symbol"></uui-symbol-expand
					></uui-button>
				</uui-input>

				<uui-scroll-container slot="popover" id="saved-searches-container" role="list">
					${this._savedSearches.map(
						(search) =>
							html`<li class="saved-search-item">
								<button
									label="Search for ${search.name}"
									class="saved-search-item-button"
									@click=${() => this.#setQueryFromSavedSearch(search.query ?? '')}>
									<span class="saved-search-item-name">${search.name}</span>
									<span class="saved-search-item-query">${search.query}</span></button
								><uui-button label="Remove saved search" color="danger"
									><uui-icon name="umb:trash"></uui-icon
								></uui-button>
							</li>`
					)}
				</uui-scroll-container>
			</uui-popover>
			<uui-button look="primary" @click=${this.#search} label="Search">Search</uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-input': UmbLogViewerSearchInputElement;
	}
}
