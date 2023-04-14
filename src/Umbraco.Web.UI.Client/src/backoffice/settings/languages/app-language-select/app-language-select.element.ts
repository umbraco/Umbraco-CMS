import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUIMenuItemEvent } from '@umbraco-ui/uui';
import { UmbLanguageRepository } from '../repository/language.repository';
import { UMB_APP_LANGUAGE_CONTEXT_TOKEN, UmbAppLanguageContext } from './app-language.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-app-language-select')
export class UmbAppLanguageSelectElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				position: relative;
				z-index: 10;
			}

			#toggle {
				display: block;
				width: 100%;
				text-align: left;
				background: none;
				border: none;
				height: 70px;
				padding: 0 var(--uui-size-8);
				border-bottom: 1px solid var(--uui-color-border);
				font-size: 14px;
				display: flex;
				align-items: center;
				justify-content: space-between;
				cursor: pointer;
			}

			#toggle:hover {
				background-color: var(--uui-color-surface-emphasis);
			}
		`,
	];

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

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT_TOKEN, (instance) => {
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

	#onClick() {
		this.#toggleDropdown();
	}

	#onClose() {
		this.#closeDropdown();
	}

	#toggleDropdown() {
		this._isOpen = !this._isOpen;

		// first start observing the languages when the dropdown is opened
		if (this._isOpen && !this.#languagesObserver) {
			this.#observeLanguages();
		}
	}

	#closeDropdown() {
		this._isOpen = false;
	}

	#onLabelClick(event: UUIMenuItemEvent) {
		const menuItem = event.target;
		const isoCode = menuItem.dataset.isoCode;

		// TODO: handle error
		if (!isoCode) return;

		this.#appLanguageContext?.setLanguage(isoCode);
		this._isOpen = false;
	}

	render() {
		return html` <umb-dropdown .open="${this._isOpen}" @close=${this.#onClose}>
			${this.#renderTrigger()} ${this.#renderContent()}
		</umb-dropdown>`;
	}

	#renderTrigger() {
		return html`<button id="toggle" slot="trigger" @click=${this.#onClick}>
			${this._appLanguage?.name} <uui-symbol-expand></uui-symbol-expand>
		</button>`;
	}

	#renderContent() {
		return html`<div slot="dropdown">
			${repeat(
				this._languages,
				(language) => language.isoCode,
				(language) =>
					html`
						<uui-menu-item
							label=${ifDefined(language.name)}
							@click-label=${this.#onLabelClick}
							data-iso-code=${ifDefined(language.isoCode)}
							?active=${language.isoCode === this._appLanguage?.isoCode}></uui-menu-item>
					`
			)}
		</div>`;
	}
}

export default UmbAppLanguageSelectElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-language-select': UmbAppLanguageSelectElement;
	}
}
