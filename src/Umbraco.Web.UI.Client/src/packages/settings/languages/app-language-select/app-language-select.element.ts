import { UmbLanguageRepository } from '../repository/language.repository.js';
import type { UmbAppLanguageContext } from './app-language.context.js';
import { UMB_APP_LANGUAGE_CONTEXT } from './app-language.context.js';
import type { UUIMenuItemEvent, UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, ifDefined, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-app-language-select')
export class UmbAppLanguageSelectElement extends UmbLitElement {
	@query('#dropdown-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _languages: Array<LanguageResponseModel> = [];

	@state()
	private _appLanguage?: LanguageResponseModel;

	@state()
	private _isOpen = false;

	#repository = new UmbLanguageRepository(this);
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

		this.observe(this.#appLanguageContext.appLanguage, (isoCode) => {
			this._appLanguage = isoCode;
		});
	}

	async #observeLanguages() {
		const { asObservable } = await this.#repository.requestLanguages();

		this.#languagesObserver = this.observe(asObservable(), (languages) => {
			this._languages = languages;
		});
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
		const isoCode = menuItem.dataset.isoCode;

		// TODO: handle error
		if (!isoCode) return;

		this.#appLanguageContext?.setLanguage(isoCode);
		this._isOpen = false;

		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
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
					(language) => language.isoCode,
					(language) => html`
						<uui-menu-item
							label=${ifDefined(language.name)}
							@click-label=${this.#onLabelClick}
							data-iso-code=${ifDefined(language.isoCode)}
							?active=${language.isoCode === this._appLanguage?.isoCode}></uui-menu-item>
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
				width: 100%;
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
