import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import type { UmbAppLanguageContext } from '../global-contexts/index.js';
import { UMB_APP_LANGUAGE_CONTEXT } from '../constants.js';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import {
	css,
	html,
	customElement,
	state,
	repeat,
	ifDefined,
	query,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

@customElement('umb-app-language-select')
export class UmbAppLanguageSelectElement extends UmbLitElement {
	@query('#dropdown-popover')
	private _popoverElement?: UUIPopoverContainerElement;

	@state()
	private _languages: Array<UmbLanguageDetailModel> = [];

	@state()
	private _appLanguage?: UmbLanguageDetailModel;

	@state()
	private _appLanguageIsReadOnly = false;

	@state()
	private _isOpen = false;

	#collectionRepository = new UmbLanguageCollectionRepository(this);
	#appLanguageContext?: UmbAppLanguageContext;
	#languagesObserver?: any;

	// TODO: Here we have some read only state logic and then we have it again in the context. We should align this otherwise it will become a nightmare to maintain. [NL]
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;

	@state()
	_disallowedLanguages: Array<UmbLanguageDetailModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguageContext = instance;
			this.#observeAppLanguage();
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context.languages, (languages) => {
				this.#currentUserAllowedLanguages = languages;
				this.#checkForLanguageAccess();
			});

			this.observe(context.hasAccessToAllLanguages, (hasAccessToAllLanguages) => {
				this.#currentUserHasAccessToAllLanguages = hasAccessToAllLanguages;
				this.#checkForLanguageAccess();
			});
		});
	}

	#checkForLanguageAccess() {
		// find all disallowed languages
		this._disallowedLanguages = this._languages?.filter((language) => {
			if (this.#currentUserHasAccessToAllLanguages) {
				return false;
			}

			return !this.#currentUserAllowedLanguages?.includes(language.unique);
		});
	}

	async #observeAppLanguage() {
		if (!this.#appLanguageContext) return;

		this.observe(this.#appLanguageContext.appLanguage, (language) => {
			this._appLanguage = language;
		});

		this.observe(this.#appLanguageContext.appLanguageReadOnlyState.isOn, (isReadOnly) => {
			this._appLanguageIsReadOnly = isReadOnly;
		});
	}

	async #observeLanguages() {
		const { data } = await this.#collectionRepository.requestCollection({});

		// TODO: listen to changes
		if (data) {
			this._languages = data.items;
			this.#checkForLanguageAccess();
		}
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._isOpen = event.newState === 'open';
		if (this._isOpen && !this.#languagesObserver) {
			if (this._popoverElement) {
				const host = this.getBoundingClientRect();
				this._popoverElement.style.width = `${host.width}px`;
			}

			this.#observeLanguages();
		}
	}

	#chooseLanguage(unique: string) {
		this.#appLanguageContext?.setLanguage(unique);
		this._isOpen = false;
		this._popoverElement?.hidePopover();
	}

	override render() {
		return html`${this.#renderTrigger()} ${this.#renderContent()}`;
	}

	#renderTrigger() {
		return html`<button id="toggle" data-mark="action:open" popovertarget="dropdown-popover">
			<span
				>${this._appLanguage?.name}
				${this._appLanguageIsReadOnly ? this.#renderReadOnlyTag(this._appLanguage?.unique) : nothing}</span
			>
			<uui-symbol-expand .open=${this._isOpen}></uui-symbol-expand>
		</button>`;
	}

	#renderContent() {
		return html` <uui-popover-container
			id="dropdown-popover"
			data-mark="app-language-menu"
			@beforetoggle=${this.#onPopoverToggle}>
			<umb-popover-layout>
				${repeat(
					this._languages,
					(language) => language.unique,
					(language) => html`
						<uui-menu-item
							label=${ifDefined(language.name)}
							data-mark="${language.entityType}:${language.unique}"
							?active=${language.unique === this._appLanguage?.unique}
							@click-label=${() => this.#chooseLanguage(language.unique)}>
							${this.#isLanguageReadOnly(language.unique) ? this.#renderReadOnlyTag(language.unique) : nothing}
						</uui-menu-item>
					`,
				)}
			</umb-popover-layout>
		</uui-popover-container>`;
	}

	#isLanguageReadOnly(culture?: string) {
		if (!culture) return false;
		return this._disallowedLanguages.find((language) => language.unique === culture) ? true : false;
	}

	#renderReadOnlyTag(culture?: string) {
		if (!culture) return nothing;
		return html`<uui-tag slot="badge" look="secondary">${this.localize.term('general_readOnly')}</uui-tag>`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				position: relative;
				z-index: 10;
			}

			#toggle {
				color: var(--uui-color-text);
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

			uui-menu-item {
				color: var(--uui-color-text);
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
