import type { UmbLogViewerWorkspaceContext } from '../../../logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { UMB_LOG_VIEWER_SAVE_SEARCH_MODAL } from './log-viewer-search-input-modal.modal-token.js';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { Subject, debounceTime, tap } from '@umbraco-cms/backoffice/external/rxjs';
import type { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { query as getQuery, path, toQueryString } from '@umbraco-cms/backoffice/router';
import { UMB_MODAL_MANAGER_CONTEXT, umbConfirmModal } from '@umbraco-cms/backoffice/modal';

import './log-viewer-search-input-modal.element.js';
import type { UmbDropdownElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-log-viewer-search-input')
export class UmbLogViewerSearchInputElement extends UmbLitElement {
	@query('#search-dropdown')
	private _searchDropdownElement!: UmbDropdownElement;

	@state()
	private _savedSearches: SavedLogSearchResponseModel[] = [];

	@state()
	private _inputQuery = '';

	@state()
	private _showLoader = false;

	@state()
	private _isQuerySaved = false;

	// TODO: Revisit this code, to not use RxJS directly:
	private inputQuery$ = new Subject<string>();

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#observeStuff();
			this.#logViewerContext?.getSavedSearches();
		});

		this.inputQuery$
			.pipe(
				tap(() => (this._showLoader = true)),
				debounceTime(250),
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
			this._savedSearches = savedSearches?.items ?? [];
			this._isQuerySaved = this._savedSearches.some((search) => search.query === this._inputQuery);
		});

		this.observe(this.#logViewerContext.filterExpression, (query) => {
			this._inputQuery = query;
			this._isQuerySaved = this._savedSearches.some((search) => search.query === query);
		});
	}

	#setQuery(event: Event) {
		const target = event.target as UUIInputElement;
		this.inputQuery$.next(target.value as string);
	}

	#setQueryFromSavedSearch(query: string) {
		this.inputQuery$.next(query);
		this._searchDropdownElement.open = false;
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

	#saveSearch(savedSearch: SavedLogSearchResponseModel) {
		this.#logViewerContext?.saveSearch(savedSearch);
	}

	async #removeSearch(name: string) {
		await umbConfirmModal(this, {
			headline: this.localize.term('logViewer_deleteSavedSearch'),
			content: `${this.localize.term('defaultdialogs_confirmdelete')} ${name}?`,
			color: 'danger',
			confirmLabel: 'Delete',
		});

		this.#logViewerContext?.removeSearch({ name });
		//this.dispatchEvent(new UmbDeleteEvent());
	}

	async #openSaveSearchDialog() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_LOG_VIEWER_SAVE_SEARCH_MODAL, {
			data: { query: this._inputQuery },
		});
		modal?.onSubmit().then((savedSearch) => {
			if (savedSearch) {
				this.#saveSearch(savedSearch);
				this._isQuerySaved = true;
			}
		});
	}

	override render() {
		return html`
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
										><uui-icon name="icon-favorite"></uui-icon
									></uui-button>`
								: ''}<uui-button compact slot="append" label="Clear" @click=${this.#clearQuery}
								><uui-icon name="icon-delete"></uui-icon
							></uui-button>`
					: html``}
				<umb-dropdown id="search-dropdown" slot="append" label=${this.localize.term('logViewer_savedSearches')}>
					<span slot="label"><umb-localize key="logViewer_savedSearches">Saved searches</umb-localize></span>
					<uui-scroll-container id="saved-searches-container" role="list">
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
										><uui-icon name="icon-trash"></uui-icon
									></uui-button>
								</li>`,
						)}
					</uui-scroll-container>
				</umb-dropdown>
			</uui-input>
		`;
	}

	static override styles = [
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
