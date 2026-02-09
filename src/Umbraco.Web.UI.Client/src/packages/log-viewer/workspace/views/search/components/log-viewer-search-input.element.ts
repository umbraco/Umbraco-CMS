import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { UMB_LOG_VIEWER_SAVE_SEARCH_MODAL } from './log-viewer-search-input-modal.modal-token.js';
import { css, html, customElement, query, state, when, nothing } from '@umbraco-cms/backoffice/external/lit';
import { escapeHTML } from '@umbraco-cms/backoffice/utils';
import { query as getQuery, path, toQueryString } from '@umbraco-cms/backoffice/router';
import { umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDropdownElement } from '@umbraco-cms/backoffice/components';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { debounceTime, skip } from '@umbraco-cms/backoffice/external/rxjs';

import './log-viewer-search-input-modal.element.js';

@customElement('umb-log-viewer-search-input')
export class UmbLogViewerSearchInputElement extends UmbLitElement {
	@query('#search-dropdown')
	private _searchDropdownElement!: UmbDropdownElement;

	@state()
	private _savedSearches: SavedLogSearchResponseModel[] = [];

	@state()
	private _inputQuery = '';

	@state()
	private _isQuerySaved = false;

	// Local state for debouncing user input before updating context
	#localQueryState = new UmbStringState('');

	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consumeContext({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value) {
		this.#logViewerContext = value;
		this.#observeStuff();
		this.#logViewerContext?.getSavedSearches();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	constructor() {
		super();

		// Debounce local input and update context
		this.observe(
			this.#localQueryState.asObservable().pipe(
				skip(1), // Skip initial value
				debounceTime(250),
			),
			(query) => {
				this._logViewerContext?.setFilterExpression(query);
				this.#persist(query);
			},
		);
	}

	#observeStuff() {
		this.observe(this._logViewerContext?.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches?.items ?? [];
			this._isQuerySaved = this._savedSearches.some((search) => search.query === this._inputQuery);
		});

		this.observe(this._logViewerContext?.filterExpression, (query) => {
			this._inputQuery = query ?? '';
			this._isQuerySaved = this._savedSearches.some((search) => search.query === this._inputQuery);
		});
	}

	#setQuery(event: Event) {
		const target = event.target as UUIInputElement;
		const query = target.value as string;
		// Update local state which will debounce before updating context
		this.#localQueryState.setValue(query);
	}

	#setQueryFromSavedSearch(query: string) {
		this._logViewerContext?.setFilterExpression(query);
		this.#persist(query);
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
		this._logViewerContext?.setFilterExpression('');
		this.#persist('');
		this.#localQueryState.setValue('');
	}

	#refreshSearch() {
		// Force immediate search, bypassing debounce
		this._logViewerContext?.getLogs();
	}

	#saveSearch(savedSearch: SavedLogSearchResponseModel) {
		this._logViewerContext?.saveSearch(savedSearch);
	}

	async #removeSearch(name: string) {
		await umbConfirmModal(this, {
			headline: this.localize.term('logViewer_deleteSavedSearch'),
			content: this.localize.term('defaultdialogs_confirmdelete', escapeHTML(name)),
			color: 'danger',
			confirmLabel: this.localize.term('actions_delete'),
		});

		this._logViewerContext?.removeSearch({ name });
		//this.dispatchEvent(new UmbDeleteEvent());
	}

	async #openSaveSearchDialog() {
		umbOpenModal(this, UMB_LOG_VIEWER_SAVE_SEARCH_MODAL, {
			data: { query: this._inputQuery },
		})
			.then((savedSearch) => {
				if (savedSearch) {
					this.#saveSearch(savedSearch);
					this._isQuerySaved = true;
				}
			})
			.catch(() => {});
	}

	override render() {
		return html`
			<uui-input
				id="search-input"
				label=${this.localize.term('logViewer_searchLogs')}
				.placeholder=${this.localize.term('logViewer_searchLogsPlaceholder')}
				slot="trigger"
				@input=${this.#setQuery}
				.value=${this._inputQuery}>
				${when(
					this._inputQuery !== '',
					() =>
						html`${when(
								this._isQuerySaved,
								() => nothing,
								() =>
									html`<uui-button
										compact
										slot="append"
										label=${this.localize.term('logViewer_saveSearch')}
										@click=${this.#openSaveSearchDialog}>
										<uui-icon name="icon-favorite"></uui-icon>
									</uui-button>`,
							)}
							<uui-button
								compact
								slot="append"
								label=${this.localize.term('logViewer_refreshSearch')}
								@click=${this.#refreshSearch}>
								<uui-icon name="icon-refresh"></uui-icon>
							</uui-button>
							<uui-button compact slot="append" label=${this.localize.term('general_clear')} @click=${this.#clearQuery}>
								<uui-icon name="icon-delete"></uui-icon>
							</uui-button>`,
					() => nothing,
				)}
				<umb-dropdown id="search-dropdown" slot="append" label=${this.localize.term('logViewer_savedSearches')}>
					<span slot="label"><umb-localize key="logViewer_savedSearches">Saved searches</umb-localize></span>
					<uui-scroll-container id="saved-searches-container" role="list">
						${this._savedSearches.map(
							(search) =>
								html`<li class="saved-search-item">
									<button
										label=${this.localize.term('logViewer_searchFor', search.name ?? '')}
										class="saved-search-item-button"
										@click=${() => this.#setQueryFromSavedSearch(search.query ?? '')}>
										<span class="saved-search-item-name">${search.name}</span>
										<span class="saved-search-item-query">${search.query}</span></button
									><uui-button
										label=${this.localize.term('logViewer_deleteThisSearch')}
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
