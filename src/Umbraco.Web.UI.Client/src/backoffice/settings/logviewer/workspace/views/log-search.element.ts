import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state, query } from 'lit/decorators.js';
import {
	LogViewerDateRange,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { LogMessageModel, SavedLogSearchModel } from '@umbraco-cms/backend-api';
import { UUIInputElement, UUIPopoverElement, UUISymbolExpandElement } from '@umbraco-ui/uui';

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

			uui-symbol-expand:not(#polling-symbol-expand) {
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
	private _startDate = '';

	@state()
	private _endDate = '';

	@state()
	private _inputQuery = '';

	@state()
	private _logs: LogMessageModel[] = [];

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

		this.observe(this.#logViewerContext.dateRange, (dateRange: LogViewerDateRange) => {
			this._startDate = dateRange?.startDate;
			this._endDate = dateRange?.endDate;
		});

		this.observe(this.#logViewerContext.currentQuery, (query) => {
			this._inputQuery = query;
		});

		this.observe(this.#logViewerContext.logs, (logs) => {
			this._logs = logs ?? [];
		});
	}

	#toggleSavedSearchesPopover() {
		this._savedSearchesPopover.open = !this._savedSearchesPopover.open;
	}

	#toggleSavedSearchesExpandSymbol() {
		this._savedSearchesExpandSymbol.open = !this._savedSearchesExpandSymbol.open;
	}

	#openPopover() {
		this.#toggleSavedSearchesPopover();
		this.#toggleSavedSearchesExpandSymbol();
	}

	#setQuery(event: Event) {
		const target = event.target as UUIInputElement;
		this._inputQuery = target.value as string;
	}

	#clearQuery() {
		this._inputQuery = '';
	}

	#renderSearchInput() {
		return html`<uui-popover
				placement="bottom-start"
				id="saved-searches-popover"
				@close=${this.#toggleSavedSearchesExpandSymbol}>
				<uui-input
					id="search-input"
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
					<uui-button compact slot="append" id="saved-searches-button" @click=${this.#openPopover}
						>Saved searches <uui-symbol-expand id="saved-search-expand-symbol"></uui-symbol-expand
					></uui-button>
				</uui-input>

				<uui-scroll-container slot="popover" id="saved-searches-container" role="list">
					${this._savedSearches.map(
						(search) =>
							html`<li class="saved-search-item">
								<button label="Search for ${search.name}" class="saved-search-item-button">
									<span class="saved-search-item-name">${search.name}</span>
									<span class="saved-search-item-query">${search.query}</span></button
								><uui-button label="Remove saved search" color="danger"
									><uui-icon name="umb:trash"></uui-icon
								></uui-button>
							</li>`
					)}
				</uui-scroll-container>
			</uui-popover>
			<uui-button look="primary">Search</uui-button>`;
	}

	render() {
		return html`
			<div id="layout">
				<div id="levels-container">
					<uui-button>Log level: All <uui-symbol-expand></uui-symbol-expand></uui-button>
					<uui-button-group>
						<uui-button>Polling</uui-button>
						<uui-button compact><uui-symbol-expand id="polling-symbol-expand"></uui-symbol-expand></uui-button>
					</uui-button-group>
				</div>
				<div id="input-container">${this.#renderSearchInput()}</div>
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
