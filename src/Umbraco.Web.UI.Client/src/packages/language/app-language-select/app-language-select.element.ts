import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import type { UmbAppLanguageContext } from './app-language.context.js';
import { UMB_APP_LANGUAGE_CONTEXT } from './app-language.context.js';
import type { UUIMenuItemEvent, UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, ifDefined, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-app-language-select')
export class UmbAppLanguageSelectElement extends UmbLitElement {
	@query('#dropdown-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _languages: Array<UmbLanguageDetailModel> = [];

	@state()
	private _appLanguage?: UmbLanguageDetailModel;

	@state()
	private _isOpen = false;

	#collectionRepository = new UmbLanguageCollectionRepository(this);
	#appLanguageContext?: UmbAppLanguageContext;
	#languagesObserver?: any;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguageContext = instance;
			this.#observeAppLanguage();
		});
	}

	async #observeAppLanguage() {
		if (!this.#appLanguageContext) return;

		this.observe(this.#appLanguageContext.appLanguage, (language) => {
			this._appLanguage = language;
		});
	}

	async #observeLanguages() {
		const { data } = await this.#collectionRepository.requestCollection({ skip: 0, take: 1000 });

		// TODO: listen to changes
		if (data) {
			this._languages = data.items;
		}
	}

	// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._isOpen = event.newState === 'open';
		if (this._isOpen && !this.#languagesObserver) {
			if (this._popoverElement) {
				const host = this.getBoundingClientRect();
				this._popoverElement.style.width = `${host.width}px`;
			}

			this.#observeLanguages();
		}
	}

	#onLabelClick(event: UUIMenuItemEvent) {
		const menuItem = event.target;
		const unique = menuItem.dataset.unique;
		if (!unique) throw new Error('Missing unique on menu item');

		this.#appLanguageContext?.setLanguage(unique);
		this._isOpen = false;

		this._popoverElement?.hidePopover();
	}

	render() {
		return html`${this.#renderTrigger()} ${this.#renderContent()}`;
	}

	#renderTrigger() {
		return html`<button id="toggle" popovertarget="dropdown-popover">
			${this._appLanguage?.name} <uui-symbol-expand .open=${this._isOpen}></uui-symbol-expand>
		</button>`;
	}

	#renderContent() {
		return html` <uui-popover-container id="dropdown-popover" @beforetoggle=${this.#onPopoverToggle}>
			<umb-popover-layout>
				${repeat(
					this._languages,
					(language) => language.unique,
					(language) => html`
						<uui-menu-item
							label=${ifDefined(language.name)}
							@click-label=${this.#onLabelClick}
							data-iso-code=${ifDefined(language.unique)}
							?active=${language.unique === this._appLanguage?.unique}></uui-menu-item>
					`,
				)}
			</umb-popover-layout>
		</uui-popover-container>`;
	}

	static styles = [
		css`
			:host {
				display: block;
				position: relative;
				z-index: 10;
			}

			#toggle {
				width: var(--umb-section-sidebar-width);
				text-align: left;
				background: none;
				border: none;
				height: var(--umb-header-layout-height);
				padding: 0 var(--uui-size-8);
				border-bottom: 1px solid var(--uui-color-border);
				font-size: 14px;
				display: flex;
				align-items: center;
				justify-content: space-between;
				cursor: pointer;
				font-family: inherit;
			}

			#toggle:hover {
				background-color: var(--uui-color-surface-emphasis);
			}
		`,
	];
}

export default UmbAppLanguageSelectElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-language-select': UmbAppLanguageSelectElement;
	}
}
