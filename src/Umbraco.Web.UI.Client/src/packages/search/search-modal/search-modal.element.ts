import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, repeat, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestSearchResultItem } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

import '../search-result/search-result-item.element.js';

export type SearchItem = {
	entityType: string;
};
export type SearchGroupItem = {
	name: string;
	items: Array<SearchItem>;
};
@customElement('umb-search-modal')
export class UmbSearchModalElement extends UmbLitElement {
	@query('input')
	private _input!: HTMLInputElement;

	@state()
	private _search = '';

	@state()
	private _searchResults: Array<SearchItem> = [];

	@state()
	private searchTags: Array<string> = [
		'Data Type',
		'Document',
		'Document Type',
		'Media',
		'Media Type',
		'Member',
		'Member Type',
		'Users',
		'User Group',
	];

	@state()
	private _activeSearchTag = 'Document';

	@state()
	private _searchProviders: Array<{
		name: string;
		providerPromise: any;
		alias: string;
	}> = [];

	#currentProvider: any;

	/**
	 *
	 */
	constructor() {
		super();

		this.#observeViews();
	}

	#observeViews() {
		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'searchProvider', null, (providers) => {
			this._searchProviders = providers.map((provider) => ({
				name: provider.manifest.meta?.label || provider.manifest.name,
				providerPromise: createExtensionApi(this, provider.manifest),
				alias: provider.alias,
			}));
		});
	}

	connectedCallback() {
		super.connectedCallback();

		requestAnimationFrame(() => {
			this._input.focus();
		});
	}

	#onSearchChange(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		this._search = target.value;

		this.#updateSearchResults();
	}

	async #onSearchTagClick(searchProvider: any) {
		console.log(searchProvider);
		const api = await searchProvider.providerPromise;

		this.#currentProvider = api;
		this._activeSearchTag = searchProvider.alias;
	}

	async #updateSearchResults() {
		if (this._search) {
			const { data, error } = await this.#currentProvider.search({
				query: this._search,
			});

			console.log('data', data);

			if (!data) return;

			this._searchResults = data.items;

			console.log('reults', this._searchResults);
			// this._searchResults = this.#mockApi.getDocuments.filter((item) =>
			// 	item.name.toLowerCase().includes(this._search.toLowerCase()),
			// );
		} else {
			this._searchResults = [];
		}
	}

	render() {
		return html`
			<div id="top">
				<div id="search-icon">
					<uui-icon name="search"></uui-icon>
				</div>
				<input
					value=${this._search}
					@input=${this.#onSearchChange}
					type="text"
					placeholder="Search..."
					autocomplete="off" />
			</div>

			${this.#renderSearchTags()}
			${this._search
				? html`<div id="main">${this._searchResults.length > 0 ? this.#renderResults() : this.#renderNoResults()}</div>`
				: nothing}
		`;
	}

	#renderSearchTags() {
		return html`<div id="search-tags">
			${repeat(
				this._searchProviders,
				(searchProvider) => searchProvider,
				(searchProvider) =>
					html`<button
						@click=${() => this.#onSearchTagClick(searchProvider)}
						@keydown=${() => ''}
						class="search-tag ${this._activeSearchTag === searchProvider.alias ? 'active' : ''}">
						${searchProvider.name}
					</button>`,
			)}
		</div> `;
	}

	#renderResults() {
		return repeat(
			this._searchResults,
			(item) => item.name,
			(item) => this.#renderItem(item),
		);
	}

	#renderNoResults() {
		return html`<div id="no-results">Only mock data for now <strong>Search for blog</strong></div>`;
	}

	#renderItem(item: SearchItem) {
		return html`
			<umb-extension-slot
				type="searchResultItem"
				.props=${{ item }}
				.filter=${(manifest: ManifestSearchResultItem) => manifest.forEntityTypes.includes(item.entityType)}
				default-element="umb-search-result-item"></umb-extension-slot>
		`;
		return html`
			<a href="${item.href}" class="item">
				<span class="item-icon">
					${item.icon ? html`<umb-icon name="${item.icon}"></umb-icon>` : this.#renderHashTag()}
				</span>
				<span class="item-name">
					${item.name} ${item.url ? html`<span class="item-url">${item.url}</span>` : nothing}
				</span>
				<span class="item-symbol">></span>
			</a>
		`;
	}

	#renderHashTag() {
		return html`
			<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
				<path fill="none" d="M0 0h24v24H0z" />
				<path
					fill="currentColor"
					d="M7.784 14l.42-4H4V8h4.415l.525-5h2.011l-.525 5h3.989l.525-5h2.011l-.525 5H20v2h-3.784l-.42 4H20v2h-4.415l-.525 5h-2.011l.525-5H9.585l-.525 5H7.049l.525-5H4v-2h3.784zm2.011 0h3.99l.42-4h-3.99l-.42 4z" />
			</svg>
		`;
	}

	#mockApi = {
		getDocuments: [
			{
				name: 'Blog',
				href: '#',
				icon: 'icon-thumbnail-list',
				url: '/blog/',
			},
			{
				name: 'Popular blogs',
				href: '#',
				icon: 'icon-article',
				url: '/blog/popular-blogs/',
			},
			{
				name: 'How to write a blog',
				href: '#',
				icon: 'icon-article',
				url: '/blog/how-to-write-a-blog/',
			},
		],
		getMedia: [
			{
				name: 'Blog hero',
				href: '#',
				icon: 'icon-picture',
			},
		],
		getDocumentTypes: [
			{
				name: 'Contact form for blog',
				href: '#',
			},
			{
				name: 'Blog',
				href: '#',
			},
			{
				name: 'Blog link item',
				href: '#',
			},
		],
	};

	static styles = [
		UmbTextStyles,
		css`
			#search-tags {
				display: flex;
				flex-wrap: wrap;
				gap: var(--uui-size-space-2);
				padding: 0 var(--uui-size-space-5);
				padding-bottom: var(--uui-size-space-2);
			}
			.search-tag {
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				background: var(--uui-color-surface-alt);
				line-height: 1;
				white-space: nowrap;
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-interactive);
				cursor: pointer;
				border: 2px solid transparent;
			}
			.search-tag:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.search-tag.active {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
				border-color: var(--uui-color-focus);
			}
			:host {
				display: flex;
				flex-direction: column;
				width: min(600px, 100vw);
				height: 100%;
				background-color: var(--uui-color-surface);
				box-sizing: border-box;
				color: var(--uui-color-text);
				font-size: 1rem;
				padding-bottom: var(--uui-size-space-2);
			}
			input {
				all: unset;
				height: 100%;
				width: 100%;
			}
			button {
				font-family: unset;
				font-size: unset;
				cursor: pointer;
			}
			#search-icon {
				display: flex;
				align-items: center;
				justify-content: center;
				aspect-ratio: 1;
				height: 100%;
			}
			#top {
				background-color: var(--uui-color-surface);
				display: flex;
				height: 48px;
			}
			#main {
				display: flex;
				flex-direction: column;
				height: 100%;
			}
			.item {
				background: var(--uui-color-surface);
				padding: var(--uui-size-space-3) var(--uui-size-space-5);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-interactive);
				display: grid;
				grid-template-columns: var(--uui-size-space-6) 1fr var(--uui-size-space-5);
				align-items: center;
			}
			.item:hover {
				background-color: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.item:hover .item-symbol {
				font-weight: unset;
				opacity: 1;
			}
			.item-icon {
				margin-bottom: auto;
				margin-top: 5px;
			}
			.item-icon,
			.item-symbol {
				opacity: 0.4;
			}
			.item-url {
				font-size: 0.8rem;
				line-height: 1.2;
				font-weight: 100;
			}
			.item-name {
				display: flex;
				flex-direction: column;
			}
			.item-icon > * {
				height: 1rem;
				display: flex;
				width: min-content;
			}
			.item-symbol {
				font-weight: 100;
			}
			a {
				text-decoration: none;
				color: inherit;
			}
			#no-results {
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				height: 100%;
				width: 100%;
				margin-top: var(--uui-size-space-5);
				color: var(--uui-color-text-alt);
				margin: var(--uui-size-space-5) 0;
			}
		`,
	];
}

export default UmbSearchModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-modal': UmbSearchModalElement;
	}
}
