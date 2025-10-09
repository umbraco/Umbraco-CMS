import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import type { UmbAppLanguageContext } from '../global-contexts/index.js';
import { UMB_APP_LANGUAGE_CONTEXT } from '../constants.js';
import type {
	UUIComboboxListElement,
	UUIComboboxListEvent,
	UUIPopoverContainerElement,
} from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, repeat, query, nothing } from '@umbraco-cms/backoffice/external/lit';
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

	@state()
	private _disallowedLanguages: Array<UmbLanguageDetailModel> = [];

	#collectionRepository = new UmbLanguageCollectionRepository(this);
	#appLanguageContext?: UmbAppLanguageContext;
	#languagesObserver?: any;

	// TODO: Here we have some read only state logic and then we have it again in the context. We should align this otherwise it will become a nightmare to maintain. [NL]
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (instance) => {
			this.#appLanguageContext = instance;
			this.#observeAppLanguage();
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context?.languages, (languages) => {
				this.#currentUserAllowedLanguages = languages;
				this.#checkForLanguageAccess();
			});

			this.observe(context?.hasAccessToAllLanguages, (hasAccessToAllLanguages) => {
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

		this.observe(this.#appLanguageContext.appLanguageReadOnlyState.isReadOnly, (isReadOnly) => {
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

	#onBeforePopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		if (event.newState === 'open' && !this.#languagesObserver) {
			if (this._popoverElement) {
				const host = this.getBoundingClientRect();
				this._popoverElement.style.width = `${host.width}px`;
			}

			this.#observeLanguages();
		}
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._isOpen = event.newState === 'open';
	}

	#onTriggerClick() {
		if (this._isOpen) {
			this._popoverElement?.hidePopover();
		} else {
			this._popoverElement?.showPopover();
		}
		this.requestUpdate();
	}

	#onTriggerKeydown = (e: KeyboardEvent) => {
		if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
			this._popoverElement?.showPopover();
		}
		if (e.key === ' ') {
			this._popoverElement?.togglePopover();
		}
	};

	#chooseLanguage(unique: string) {
		this.#appLanguageContext?.setLanguage(unique);
		this._popoverElement?.hidePopover();
	}

	#onLanguageSelectionChange(event: UUIComboboxListEvent) {
		if (!this._isOpen) return;

		const target = event.target as UUIComboboxListElement;
		const value = target?.value as string;
		if (value) {
			this.#chooseLanguage(value);
		}
	}

	override render() {
		return html`${this.#renderTrigger()} ${this.#renderContent()}`;
	}

	#renderTrigger() {
		return html` <div
			id="toggle"
			data-mark="action:open"
			popovertarget="dropdown-popover"
			tabindex="0"
			@click=${this.#onTriggerClick}
			@keydown=${this.#onTriggerKeydown}>
			<span>
				${this._appLanguage?.name}
				${this._appLanguageIsReadOnly ? this.#renderReadOnlyTag(this._appLanguage?.unique) : nothing}
			</span>
			<uui-symbol-expand .open=${this._isOpen}></uui-symbol-expand>
		</div>`;
	}

	#renderContent() {
		return html` <uui-popover-container
			id="dropdown-popover"
			data-mark="app-language-menu"
			@beforetoggle=${this.#onBeforePopoverToggle}
			@toggle=${this.#onPopoverToggle}>
			<umb-popover-layout>
				<uui-scroll-container style="max-height:calc(100vh - (var(--umb-header-layout-height) + 60px));">
					${this.#renderOptions()}
				</uui-scroll-container>
			</umb-popover-layout>
		</uui-popover-container>`;
	}

	#renderOptions() {
		if (!this._isOpen) return nothing;

		return html`<uui-combobox-list
			aria-label="App language"
			.for=${this}
			.value=${this._appLanguage?.unique || ''}
			@change=${this.#onLanguageSelectionChange}>
			${repeat(
				this._languages,
				(language) => language.unique,
				(language) => html`
					<uui-combobox-list-option tabindex="0" value=${language.unique}>
						${language.name}
						${this.#isLanguageReadOnly(language.unique) ? this.#renderReadOnlyTag(language.unique) : nothing}
					</uui-combobox-list-option>
				`,
			)}
		</uui-combobox-list>`;
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

				width: auto;
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
