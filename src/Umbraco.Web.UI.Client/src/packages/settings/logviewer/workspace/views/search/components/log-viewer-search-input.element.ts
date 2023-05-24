import { UUIButtonElement, UUIInputElement, UUIPopoverElement, UUISymbolExpandElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { Subject, debounceTime, tap } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context.js';
import { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { query as getQuery, path, toQueryString } from '@umbraco-cms/backoffice/router';
import {
	UMB_MODAL_CONTEXT_TOKEN,
	UmbModalContext,
	UmbModalHandler,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';

import './log-viewer-search-input-modal.element.js';
export interface UmbContextSaveSearchModalData {
	query: string;
}

export const UMB_LOG_VIEWER_SAVE_SEARCH_MODAL = new UmbModalToken<UmbContextSaveSearchModalData>(
	'Umb.Modal.LogViewer.SaveSearch',
	{
		type: 'dialog',
		size: 'small',
	}
);

@customElement('umb-log-viewer-search-input')
export class UmbLogViewerSearchInputElement extends UmbLitElement {
	@query('#saved-searches-popover')
	private _savedSearchesPopover!: UUIPopoverElement;

	@query('#saved-search-expand-symbol')
	private _savedSearchesExpandSymbol!: UUISymbolExpandElement;

	@state()
	private _savedSearches: SavedLogSearchResponseModel[] = [];

	@state()
	private _inputQuery = '';

	@state()
	private _showLoader = false;

	@state()
	private _isQuerySaved = false;

	private inputQuery$ = new Subject<string>();

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
			this.#logViewerContext?.getSavedSearches();
		});

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});

		this.inputQuery$
			.pipe(
				tap(() => (this._showLoader = true)),
				debounceTime(250)
			)
			.subscribe((query) => {
				this.#logViewerContext?.setFilterExpression(query);
				this.#persist(query);
				this._isQuerySaved = this._savedSearches.some((search) => search.query === query);
				this._showLoader = false;
			});
	}

	#observeStuff() {
		if (!this.#logViewerContext) return;
		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches ?? [];
			this._isQuerySaved = this._savedSearches.some((search) => search.query === this._inputQuery);
		});

		this.observe(this.#logViewerContext.filterExpression, (query) => {
			this._inputQuery = query;
			this._isQuerySaved = this._savedSearches.some((search) => search.query === query);
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
		this.inputQuery$.next(target.value as string);
	}

	#setQueryFromSavedSearch(query: string) {
		this.inputQuery$.next(query);
		this._savedSearchesPopover.open = false;
	}

	#persist(filter: string) {
		let query = getQuery();

		query = {
			...query,
			lq: filter,
		};

		window.history.pushState({}, '', `${path()}?${toQueryString(query)}`);
	}

	#clearQuery() {
		this.inputQuery$.next('');
		this.#logViewerContext?.setFilterExpression('');
	}

	#modalHandler?: UmbModalHandler;

	#saveSearch(savedSearch: SavedLogSearchResponseModel) {
		this.#logViewerContext?.saveSearch(savedSearch);
	}

	#removeSearch(name: string) {
		this.#logViewerContext?.removeSearch({ name });
	}

	#openSaveSearchDialog() {
		this.#modalHandler = this._modalContext?.open(UMB_LOG_VIEWER_SAVE_SEARCH_MODAL, { query: this._inputQuery });
		this.#modalHandler?.onSubmit().then((savedSearch) => {
			if (savedSearch) {
				this.#saveSearch(savedSearch);
				this._isQuerySaved = true;
			}
		});
	}

	render() {
		return html`
			<uui-popover placement="bottom-start" id="saved-searches-popover" @close=${this.#toggleSavedSearchesExpandSymbol}>
				<uui-input
					id="search-input"
					label="Search logs"
					.placeholder=${'Search logs...'}
					slot="trigger"
					@input=${this.#setQuery}
					.value=${this._inputQuery}>
					${this._showLoader
						? html`<div id="loader-container" slot="append">
								<uui-loader-circle></uui-loader-circle>
						  </div>`
						: ''}
					${this._inputQuery
						? html`${!this._isQuerySaved
									? html`<uui-button compact slot="append" label="Save search" @click=${this.#openSaveSearchDialog}
											><uui-icon name="umb:favorite"></uui-icon
									  ></uui-button>`
									: ''}<uui-button compact slot="append" label="Clear" @click=${this.#clearQuery}
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
								><uui-button
									label="Remove saved search"
									color="danger"
									@click=${() => this.#removeSearch(search.name ?? '')}
									><uui-icon name="umb:trash"></uui-icon
								></uui-button>
							</li>`
					)}
				</uui-scroll-container>
			</uui-popover>
		`;
	}

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

			#loader-container {
				display: flex;
				justify-content: center;
				align-items: center;
				margin: 0 var(--uui-size-space-4);
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-search-input': UmbLogViewerSearchInputElement;
	}
}
