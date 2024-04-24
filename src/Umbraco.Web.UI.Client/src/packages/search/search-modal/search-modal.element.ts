import type { UmbSearchProvider, UmbSearchResultItemModel } from '../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	css,
	html,
	nothing,
	repeat,
	customElement,
	query,
	state,
	property,
} from '@umbraco-cms/backoffice/external/lit';
import type { ManifestSearchResultItem } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

import '../search-result/search-result-item.element.js';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

type SearchProvider = {
	name: string;
	api: UmbSearchProvider<UmbSearchResultItemModel>;
	alias: string;
};

@customElement('umb-search-modal')
export class UmbSearchModalElement extends UmbLitElement {
	@query('#input-wrapper-fake-cursor')
	private _inputFakeCursor!: HTMLElement;
	@query('input')
	private _input!: HTMLInputElement;

	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@state()
	private _search = '';

	@state()
	private _searchResults: Array<UmbSearchResultItemModel> = [];

	@state()
	private _searchProviders: Array<SearchProvider> = [];

	@state()
	_currentProvider?: SearchProvider;

	@state()
	_loading: boolean = false;

	#searchItemNavIndex = 0;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 300;

	constructor() {
		super();

		this.#observeViews();
	}

	#observeViews() {
		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'searchProvider', null, async (providers) => {
			const searchProviders: Array<SearchProvider> = [];

			for (const provider of providers) {
				const api = await createExtensionApi<UmbSearchProvider<UmbSearchResultItemModel>>(this, provider.manifest);
				if (api) {
					searchProviders.push({
						name: provider.manifest.meta?.label || provider.manifest.name,
						api,
						alias: provider.alias,
					});
				}
			}

			this._searchProviders = searchProviders;

			if (this._searchProviders.length > 0) {
				this._currentProvider = this._searchProviders[0];
			}
		});
	}

	connectedCallback() {
		super.connectedCallback();

		this.addEventListener('keydown', this.#onKeydown);

		requestAnimationFrame(() => {
			this.#focusInput();
		});
	}

	#onKeydown(event: KeyboardEvent) {
		const root = this.shadowRoot;
		if (!root) return;

		if (event.key === 'Tab') {
			const isFirstProvider = (element: Element) => element === root.querySelector('.search-provider:first-child');
			const isLastProvider = (element: Element) => element === root.querySelector('.search-provider:last-child');
			const setActiveProviderFocus = (element?: Element | null) => (element as HTMLElement)?.focus();

			const activeProvider = root.querySelector('.search-provider.active') as HTMLElement | null;

			if (!activeProvider) return;

			// When moving backwards in search providers
			if (event.shiftKey) {
				// If the FOCUS is on a provider, and it is the first in the list, we need to wrap around and focus the LAST one
				if (this.#providerHasFocus) {
					if (this.#isFocusingFirstProvider) {
						setActiveProviderFocus(root.querySelector('.search-provider:last-child'));
						event.preventDefault();
					}
					return;
				}

				// If the currently ACTIVE provider is the first in the list, we need to wrap around and focus the LAST one
				if (isFirstProvider(activeProvider)) {
					setActiveProviderFocus(root.querySelector('.search-provider:last-child'));
					event.preventDefault();
					return;
				}

				// We set the focus to current provider, and because we don't prevent the default tab behavior, the previous provider will be focused
				setActiveProviderFocus(activeProvider);
			}
			// When moving forwards in search providers
			else {
				// If the FOCUS is on a provider, and it is the last in the list, we need to wrap around and focus the FIRST one
				if (this.#providerHasFocus) {
					if (this.#isFocusingLastProvider) {
						setActiveProviderFocus(root.querySelector('.search-provider:first-child'));
						event.preventDefault();
					}
					return;
				}

				// If the currently ACTIVE provider is the last in the list, we need to wrap around and focus the FIRST one
				if (isLastProvider(activeProvider)) {
					setActiveProviderFocus(root.querySelector('.search-provider:first-child'));
					event.preventDefault();
					return;
				}

				// We set the focus to current provider, and because we don't prevent the default tab behavior, the next provider will be focused
				setActiveProviderFocus(activeProvider);
			}
		}

		switch (event.key) {
			case 'Tab':
			case 'Shift':
			case 'Escape':
			case 'Enter':
				break;
			case 'ArrowDown':
				event.preventDefault();
				this.#setSearchItemNavIndex(Math.min(this.#searchItemNavIndex + 1, this._searchResults.length - 1));
				break;
			case 'ArrowUp':
				event.preventDefault();
				this.#setSearchItemNavIndex(Math.max(this.#searchItemNavIndex - 1, 0));
				break;
			default:
				if (this._input === root.activeElement) return;
				this.#focusInput();
				break;
		}
	}

	async #setSearchItemNavIndex(index: number) {
		await this.updateComplete;

		const prevElement = this.shadowRoot?.querySelector(
			`a[data-item-index="${this.#searchItemNavIndex}"]`,
		) as HTMLElement | null;
		prevElement?.classList.remove('active');

		this.#searchItemNavIndex = index;

		const element = this.shadowRoot?.querySelector(`a[data-item-index="${index}"]`) as HTMLElement | null;
		element?.classList.add('active');

		if (!element) return;
		if (!this._searchResults.length) return;

		element.focus();
	}

	#focusInput() {
		this._input.focus();
	}

	get #providerHasFocus() {
		const providerElements = this.shadowRoot?.querySelectorAll('.search-provider') || [];
		return Array.from(providerElements).some((element) => element === this.shadowRoot?.activeElement);
	}

	get #isFocusingLastProvider() {
		const providerElements = this.shadowRoot?.querySelectorAll('.search-provider') || [];
		return providerElements[providerElements.length - 1] === this.shadowRoot?.activeElement;
	}

	get #isFocusingFirstProvider() {
		const providerElements = this.shadowRoot?.querySelectorAll('.search-provider') || [];
		return providerElements[0] === this.shadowRoot?.activeElement;
	}

	#onSearchChange(event: InputEvent) {
		const target = event.target as HTMLInputElement;
		this._search = target.value.trim();

		clearTimeout(this.#inputTimer);
		if (!this._search) {
			this._loading = false;
			this._searchResults = [];
			return;
		}

		this._loading = true;
		this.#inputTimer = setTimeout(() => this.#updateSearchResults(), this.#inputTimerAmount);
	}

	async #setShowFakeCursor(show: boolean) {
		if (show) {
			await new Promise((resolve) => requestAnimationFrame(resolve));
			const getTextBeforeCursor = this._search.substring(0, this._input.selectionStart ?? 0);
			this._inputFakeCursor.textContent = getTextBeforeCursor;
			this._inputFakeCursor.style.display = 'block';
		} else {
			this._inputFakeCursor.style.display = 'none';
		}
	}

	#setCurrentProvider(searchProvider: SearchProvider) {
		if (this._currentProvider === searchProvider) return;

		this._currentProvider = searchProvider;

		this.#focusInput();
		this._loading = true;
		this._searchResults = [];
		this.#updateSearchResults();
	}

	async #updateSearchResults() {
		if (this._search && this._currentProvider?.api) {
			const { data } = await this._currentProvider.api.search({ query: this._search });
			if (!data) return;
			this._searchResults = data.items;
		} else {
			this._searchResults = [];
		}

		this._loading = false;
		this.#searchItemNavIndex = -1;
	}

	render() {
		return html`
			<div id="top">
				${this.#renderSearchIcon()}
				<div id="input-wrapper">
					<div id="input-wrapper-fake-cursor" aria-hidden="true"></div>
					<input
						value=${this._search}
						@input=${this.#onSearchChange}
						@blur=${() => this.#setShowFakeCursor(true)}
						@focus=${() => this.#setShowFakeCursor(false)}
						type="text"
						placeholder="Search..."
						autocomplete="off" />
				</div>
			</div>

			${this.#renderSearchTags()}
			${this._search
				? html`<div id="main">${this._searchResults.length > 0 ? this.#renderResults() : this.#renderNoResults()}</div>`
				: nothing}
		`;
	}

	#renderSearchIcon() {
		return html` <div id="search-icon">
			${this._loading ? html`<uui-loader-circle></uui-loader-circle>` : html`<uui-icon name="search"></uui-icon>`}
		</div>`;
	}

	#renderSearchTags() {
		return html`<div id="search-providers">
			${repeat(
				this._searchProviders,
				(searchProvider) => searchProvider,
				(searchProvider) =>
					html`<button
						data-provider-alias=${searchProvider.alias}
						@click=${() => this.#setCurrentProvider(searchProvider)}
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
			(item, index) => this.#renderResultItem(item, index),
		);
	}

	#renderResultItem(item: UmbSearchResultItemModel, index: number) {
		return html`
			<a
				href=${item.href}
				data-item-index=${index}
				class="search-item"
				@click=${this.#closeModal}
				@keydown=${this.#closeModal}>
				<umb-extension-slot
					type="searchResultItem"
					.props=${{ item }}
					.filter=${(manifest: ManifestSearchResultItem) => manifest.forEntityTypes.includes(item.entityType)}
					default-element="umb-search-result-item"></umb-extension-slot>
			</a>
		`;
	}

	#renderNoResults() {
		return this._loading ? nothing : html`<div id="no-results">${this.localize.term('general_searchNoResult')}</div>`;
	}

	#closeModal(event: MouseEvent | KeyboardEvent) {
		if (event instanceof KeyboardEvent && event.key !== 'Enter') return;

		requestAnimationFrame(() => {
			// In the case where the browser has not triggered focus-visible and we keyboard navigate and press enter.
			// It is necessary to wait one frame.
			this.modalContext?.reject();
		});
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				width: min(610px, 100vw);
				height: max(600px, 80dvh);
				max-height: 100dvh;
				background-color: var(--uui-color-surface);
				box-sizing: border-box;
				color: var(--uui-color-text);
				font-size: 1rem;
				padding-bottom: var(--uui-size-space-2);
			}
			#top {
				background-color: var(--uui-color-surface);
				display: flex;
				height: 48px;
				flex-shrink: 0;
			}
			#main {
				display: flex;
				flex-direction: column;
				height: 100%;
				overflow: auto;
			}
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
				background: var(--uui-color-focus);
				color: var(--uui-color-selected-contrast);
				border-color: transparent;
			}
			.search-provider.active:focus {
				outline-offset: -4px;
				outline-color: var(--uui-color-focus);
			}
			input {
				all: unset;
				height: 100%;
				width: 100%;
			}
			#input-wrapper {
				width: 100%;
				position: relative;
			}
			#input-wrapper-fake-cursor {
				position: absolute;
				left: 0;
				border-right: 1px solid var(--uui-color-text);
				height: 1.2rem;
				color: transparent;
				user-select: none;
				pointer-events: none;
				bottom: 14px;
				animation: blink-animation 1s infinite;
			}
			@keyframes blink-animation {
				0%,
				50% {
					border-color: var(--uui-color-text);
				}
				51%,
				100% {
					border-color: transparent;
				}
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
			.search-item {
				color: var(--uui-color-text);
				text-decoration: none;
				outline-offset: -3px;
				display: flex;
			}
			.search-item:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
			}
			.search-item:focus {
				outline: 2px solid var(--uui-color-interactive-emphasis);
				border-radius: 6px;
				outline-offset: -4px;
			}
			.search-item.active:not(:focus-within) {
				outline: 2px solid var(--uui-color-border);
				border-radius: 6px;
				outline-offset: -4px;
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
