import type { UmbSearchProvider, UmbSearchResultItemModel } from '../types.js';
import type { ManifestSearchResultItem } from '../extensions/types.js';
import type { UmbGlobalSearchApi } from '../global-search/types.js';
import {
	css,
	html,
	nothing,
	repeat,
	customElement,
	query,
	state,
	property,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionsManifestInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { createExtensionApiByAlias, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

import '../search-result/search-result-item.element.js';
import { UMB_BACKOFFICE_CONTEXT } from '../../../../apps/backoffice/backoffice.context.js';

type GlobalSearchers = {
	name: string;
	api?: UmbSearchProvider<UmbSearchResultItemModel>;
	alias: string;
	sectionAlias?: string;
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
	private _globalSearchers: Array<GlobalSearchers> = [];

	@state()
	private _currentGlobalSearcher?: GlobalSearchers;

	@state()
	private _loading: boolean = false;

	#searchItemNavIndex = 0;

	#inputTimer?: NodeJS.Timeout;
	#inputTimerAmount = 300;

	#currentSectionAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BACKOFFICE_CONTEXT, (backofficeContext) => {
			if (!backofficeContext) return;
			this.observe(backofficeContext.activeSectionAlias, (alias) => {
				this.#currentSectionAlias = alias;
				this.#updateDefaultSearcher();
			});
		});

		this.#observeGlobalSearchers();
	}

	override connectedCallback() {
		super.connectedCallback();

		this.addEventListener('keydown', this.#onKeydown);
		// TODO: Temp solution to close the modal on outside click. We need to look into a generic solution for this. [JMJ]
		document.addEventListener('click', this.#onDocumentClick);

		requestAnimationFrame(() => {
			this.#focusInput();
		});
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();

		this.removeEventListener('keydown', this.#onKeydown);
		document.removeEventListener('click', this.#onDocumentClick);
	}

	#onDocumentClick = (event: MouseEvent) => {
		const path = event.composedPath();
		if (path.includes(this)) return;

		this.modalContext?.reject();
	};

	#observeGlobalSearchers() {
		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'globalSearch', null, async (controllers) => {
			const globalSearch: Array<GlobalSearchers> = [];

			for (const controller of controllers) {
				let searchApi = undefined;

				if (controller.manifest.api) {
					searchApi = await createExtensionApi<UmbGlobalSearchApi>(this, controller.manifest);

					if (searchApi) {
						// TODO: we need to investigate if it makes sense to have a function that does this when creating a new extension api? [MR]
						(searchApi as any).manifest = controller.manifest;
					}
				} else {
					searchApi = await createExtensionApiByAlias<UmbSearchProvider<UmbSearchResultItemModel>>(
						this,
						controller.manifest.meta?.searchProviderAlias,
					);
				}

				if (searchApi) {
					const searcher: GlobalSearchers = {
						name: controller.manifest.meta?.label || controller.manifest.name,
						api: searchApi,
						alias: controller.alias,
						sectionAlias: this.#extractSectionAlias(controller.manifest.conditions),
					};

					globalSearch.push(searcher);
				}
			}

			this._globalSearchers = globalSearch;
			this.#updateDefaultSearcher();
		});
	}

	#extractSectionAlias(conditions?: UmbExtensionConditionConfig[]): string | undefined {
		if (!conditions) return undefined;
		const sectionCondition = conditions.find((c) => c.alias === UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS);
		if (sectionCondition && 'match' in sectionCondition && typeof sectionCondition.match === 'string') {
			return sectionCondition.match;
		}

		return undefined;
	}

	#updateDefaultSearcher() {
		if (this._globalSearchers.length === 0) return;

		// Try to find a searcher matching the current section
		if (this.#currentSectionAlias) {
			const matchingSearcher = this._globalSearchers.find((s) => s.sectionAlias === this.#currentSectionAlias);
			if (matchingSearcher) {
				this._currentGlobalSearcher = matchingSearcher;
				return;
			}
		}

		// Fall back to first searcher
		this._currentGlobalSearcher = this._globalSearchers[0];
	}

	async #setSearchItemNavIndex(index: number) {
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

	#setCurrentSearcher(searcher: GlobalSearchers) {
		if (this._currentGlobalSearcher === searcher) return;

		this._currentGlobalSearcher = searcher;

		this.#focusInput();
		this._loading = true;
		this._searchResults = [];
		this.#updateSearchResults();
	}

	async #updateSearchResults() {
		if (this._search && this._currentGlobalSearcher?.api) {
			const { data } = await this._currentGlobalSearcher.api.search({ query: this._search });
			if (!data) return;
			this._searchResults = data.items;
		} else {
			this._searchResults = [];
		}

		this._loading = false;
		this.#searchItemNavIndex = -1;
	}

	#closeModal(event: MouseEvent | KeyboardEvent) {
		if (event instanceof KeyboardEvent && event.key !== 'Enter') return;

		requestAnimationFrame(() => {
			// In the case where the browser has not triggered focus-visible and we keyboard navigate and press enter.
			// It is necessary to wait one frame.
			this.modalContext?.reject();
		});
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

	#onKeydown(event: KeyboardEvent) {
		const root = this.shadowRoot;
		if (!root) return;

		if (event.key === 'Tab') {
			const isFirstProvider = (element: Element) => element === root.querySelector('.search-provider:first-child');
			const isLastProvider = (element: Element) => element === root.querySelector('.search-provider:last-child');
			const setFocus = (element?: Element | null) => (element as HTMLElement)?.focus();
			const providerHasFocus = () => {
				const providerElements = root.querySelectorAll('.search-provider') || [];
				return Array.from(providerElements).some((element) => element === root.activeElement);
			};
			const isFocusingLastProvider = () => {
				const providerElements = root.querySelectorAll('.search-provider') || [];
				return providerElements[providerElements.length - 1] === root.activeElement;
			};
			const isFocusingFirstProvider = () => {
				const providerElements = root.querySelectorAll('.search-provider') || [];
				return providerElements[0] === root.activeElement;
			};

			const activeProvider = root.querySelector('.search-provider.active') as HTMLElement | null;

			if (!activeProvider) return;

			// When moving backwards in search providers
			if (event.shiftKey) {
				// If the FOCUS is on a provider, and it is the first in the list, we need to wrap around and focus the LAST one
				if (providerHasFocus()) {
					if (isFocusingFirstProvider()) {
						setFocus(root.querySelector('.search-provider:last-child'));
						event.preventDefault();
					}
					return;
				}

				// If the currently ACTIVE provider is the first in the list, we need to wrap around and focus the LAST one
				if (isFirstProvider(activeProvider)) {
					setFocus(root.querySelector('.search-provider:last-child'));
					event.preventDefault();
					return;
				}

				// We set the focus to current provider, and because we don't prevent the default tab behavior, the previous provider will be focused
				setFocus(activeProvider);
			}
			// When moving forwards in search providers
			else {
				// If the FOCUS is on a provider, and it is the last in the list, we need to wrap around and focus the FIRST one
				if (providerHasFocus()) {
					if (isFocusingLastProvider()) {
						setFocus(root.querySelector('.search-provider:first-child'));
						event.preventDefault();
					}
					return;
				}

				// If the currently ACTIVE provider is the last in the list, we need to wrap around and focus the FIRST one
				if (isLastProvider(activeProvider)) {
					setFocus(root.querySelector('.search-provider:first-child'));
					event.preventDefault();
					return;
				}

				// We set the focus to current provider, and because we don't prevent the default tab behavior, the next provider will be focused
				setFocus(activeProvider);
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

	override render() {
		return html`
			<div id="top">
				<div id="search-icon">
					${when(
						this._loading,
						() => html`<uui-loader-circle></uui-loader-circle>`,
						() => html`<uui-icon name="search"></uui-icon>`,
					)}
				</div>
				<div id="input-wrapper">
					<div id="input-wrapper-fake-cursor" aria-hidden="true"></div>
					<input
						type="text"
						autocomplete="off"
						placeholder=${this.localize.term('placeholders_search')}
						value=${this._search}
						@input=${this.#onSearchChange}
						@blur=${() => this.#setShowFakeCursor(true)}
						@focus=${() => this.#setShowFakeCursor(false)} />
				</div>
			</div>
			${this.#renderSearchTags()}
			${when(
				this._search,
				() => html`
					<uui-scroll-container>
						<div id="main">
							${when(
								this._searchResults.length > 0,
								() => this.#renderResults(),
								() => this.#renderNoResults(),
							)}
						</div>
					</uui-scroll-container>
				`,
				() => this.#renderNavigationTips(),
			)}
		`;
	}

	#renderSearchTags() {
		return html`
			<div id="search-providers">
				${repeat(
					this._globalSearchers,
					(searcher) => searcher.alias,
					(searcher) => html`
						<button
							class="search-provider ${this._currentGlobalSearcher?.alias === searcher.alias ? 'active' : ''}"
							data-provider-alias=${searcher.alias}
							@click=${() => this.#setCurrentSearcher(searcher)}
							@keydown=${() => ''}>
							${searcher.name}
						</button>
					`,
				)}
			</div>
		`;
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
				class="search-item"
				data-item-index=${index}
				href=${item.href}
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
		if (this._loading) return nothing;
		return html`<div id="no-results">${this.localize.term('general_searchNoResult')}</div>`;
	}

	#renderNavigationTips() {
		return html`<div id="navigation-tips">
			<div class="navigation-tips-key" style="grid-column: span 2;">Tab</div>
			<span>${this.localize.term('globalSearch_navigateSearchProviders')}</span>
			<div class="navigation-tips-key">
				<uui-icon name="icon-arrow-up"></uui-icon>
			</div>
			<div class="navigation-tips-key">
				<uui-icon name="icon-arrow-down"></uui-icon>
			</div>
			<span>${this.localize.term('globalSearch_navigateSearchResults')}</span>
		</div>`;
	}

	static override styles = [
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
				padding-bottom: var(--uui-size-space-2);
			}

			#navigation-tips {
				display: grid;
				grid-template-columns: 50px 50px auto;
				column-gap: var(--uui-size-space-3);
				row-gap: var(--uui-size-space-4);
				align-items: center;
				color: var(--uui-color-border-emphasis);
				margin-top: var(--uui-size-layout-3);
				margin-inline: auto;
			}

			.navigation-tips-key {
				display: flex;
				align-items: center;
				justify-content: center;
				border-radius: var(--uui-border-radius);
				border: 1px solid var(--uui-color-border);
				height: 36px;
				font-size: 0.9rem;
				font-weight: bold;
			}

			#navigation-tips .navigation-tips-key + span {
				margin-left: var(--uui-size-space-2);
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
