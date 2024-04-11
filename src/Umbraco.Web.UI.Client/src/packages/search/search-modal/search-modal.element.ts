import type { UmbSearchResultItemModel } from '../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, repeat, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestSearchResultItem } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

import '../search-result/search-result-item.element.js';

@customElement('umb-search-modal')
export class UmbSearchModalElement extends UmbLitElement {
	@query('input')
	private _input!: HTMLInputElement;

	@state()
	private _search = '';

	@state()
	private _searchResults: Array<UmbSearchResultItemModel> = [];

	@state()
	private _searchProviders: Array<{
		name: string;
		providerPromise: any;
		alias: string;
	}> = [];

	@state()
	_currentProvider?: {
		api: any;
		alias: string;
	};

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
		const api = await searchProvider.providerPromise;
		this._currentProvider = {
			api,
			alias: searchProvider.alias,
		};
	}

	async #updateSearchResults() {
		if (this._search && this._currentProvider) {
			const { data } = await this._currentProvider.api.search({ query: this._search });
			if (!data) return;
			this._searchResults = data.items;
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
		return html`<div id="search-providers">
			${repeat(
				this._searchProviders,
				(searchProvider) => searchProvider,
				(searchProvider) =>
					html`<button
						@click=${() => this.#onSearchTagClick(searchProvider)}
						@keydown=${() => ''}
						class="search-provider ${this._currentProvider?.alias === searchProvider.alias ? 'active' : ''}">
						${searchProvider.name}
					</button>`,
			)}
		</div> `;
	}

	#renderResults() {
		return repeat(
			this._searchResults,
			(item) => item.unique,
			(item) => this.#renderResultItem(item),
		);
	}

	#renderResultItem(item: UmbSearchResultItemModel) {
		return html`
			<umb-extension-slot
				type="searchResultItem"
				.props=${{ item }}
				.filter=${(manifest: ManifestSearchResultItem) => manifest.forEntityTypes.includes(item.entityType)}
				default-element="umb-search-result-item"></umb-extension-slot>
		`;
	}

	#renderNoResults() {
		return html`<div id="no-results">No results found</div>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#search-providers {
				display: flex;
				flex-wrap: wrap;
				gap: var(--uui-size-space-2);
				padding: 0 var(--uui-size-space-5);
				padding-bottom: var(--uui-size-space-2);
			}
			.search-provider {
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				background: var(--uui-color-surface-alt);
				line-height: 1;
				white-space: nowrap;
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-interactive);
				cursor: pointer;
				border: 2px solid transparent;
			}
			.search-provider:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.search-provider.active {
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
