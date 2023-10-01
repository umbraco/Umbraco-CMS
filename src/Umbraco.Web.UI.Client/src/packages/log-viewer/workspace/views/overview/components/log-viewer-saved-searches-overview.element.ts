import { UmbLogViewerWorkspaceContext, UMB_APP_LOG_VIEWER_CONTEXT_TOKEN } from '../../../logviewer.context.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {debounceTime, Subject, tap} from "@umbraco-cms/backoffice/external/rxjs";
import {path, query as getQuery, toQueryString} from "@umbraco-cms/backoffice/external/router-slot";

//TODO: implement the saved searches pagination when the API total bug is fixed
@customElement('umb-log-viewer-saved-searches-overview')
export class UmbLogViewerSavedSearchesOverviewElement extends UmbLitElement {
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

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#logViewerContext?.getSavedSearches();
			this.#observeStuff();
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

	#persist(filter: string) {
		let query = getQuery();

		query = {
			...query,
			lq: filter,
		};
		const newPath = path().replace('/overview', '/search');

		window.history.pushState({}, '', `${newPath}?${toQueryString(query)}`);
	}

	#setQueryFromSavedSearch(query: string) {
		this.inputQuery$.next(query);
	}

	#renderSearchItem = (searchListItem: SavedLogSearchResponseModel) => {
		return html` <li>
			<uui-menu-item
				label="${searchListItem.name ?? ''}"
				title="${searchListItem.name ?? ''}"
				@click=${() => this.#setQueryFromSavedSearch(searchListItem.query ?? '')}>
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
