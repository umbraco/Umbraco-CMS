import { UmbLanguageCollectionRepository } from '../collection/index.js';
import type { UmbLanguageDetailModel } from '../types.js';
import type { UmbAppLanguageContext } from '../global-contexts/index.js';
import { UMB_APP_LANGUAGE_CONTEXT } from '../constants.js';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
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

	async #onBeforePopoverToggle(event: ToggleEvent) {
		if (event.newState === 'open' && !this.#languagesObserver) {
			if (this._popoverElement) {
				const host = this.getBoundingClientRect();
				this._popoverElement.style.width = `${host.width}px`;
			}

			this.#observeLanguages();
		}
	}

	async #onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._isOpen = event.newState === 'open';
	}

	#onTriggerKeydown = (e: KeyboardEvent) => {
		// Open popover on arrow up, arrow down, and space
		if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
			this.#showPopover();
			return;
		}

		if (e.key === ' ') {
			this.#togglePopover();
			return;
		}
	};

	#chooseLanguage(unique: string) {
		this.#appLanguageContext?.setLanguage(unique);
		this.#hidePopover();
	}

	#showPopover() {
		this._popoverElement?.showPopover();
	}

	#hidePopover() {
		this._popoverElement?.hidePopover();
	}

	#togglePopover() {
		this._popoverElement?.togglePopover();
	}

	#onLanguageSelectionChange(e: CustomEvent) {
		// Hack: Prevent a selection from being made when the dropdown is closed,
		// TODO: investigate why the uui-combobox-list can select a value when no longer in the DOM
		if (this._isOpen === false) return;

		const target = e.target as any;
		const value = target.value;
		this.#chooseLanguage(value);
	}

	#onTriggerClick(e: PointerEvent) {
		if (this._isOpen) {
			this.#hidePopover();
		} else {
			this.#showPopover();
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
			@keydown=${this.#onTriggerKeydown}
			@click=${this.#onTriggerClick}
			tabindex="0">
			<span>
				${this._appLanguage?.name}
				${this._appLanguageIsReadOnly ? this.#renderReadOnlyTag(this._appLanguage?.unique) : nothing}
			</span>
			<uui-symbol-expand .open=${this._isOpen}></uui-symbol-expand>
		</div>`;
	}

	#renderContent() {
		return html`<uui-popover-container
			id="dropdown-popover"
			data-mark="app-language-menu"
			@beforetoggle=${this.#onBeforePopoverToggle}
			@toggle=${this.#onPopoverToggle}>
			<umb-popover-layout>
				<uui-scroll-container style="max-height:calc(100vh - (var(--umb-header-layout-height) + 60px));">
					${this._isOpen
						? html`
								<uui-combobox-list
									.value=${this._appLanguage?.unique || ''}
									aria-label="App language"
									.for=${this}
									@change=${this.#onLanguageSelectionChange}>
									${repeat(
										this._languages,
										(language) => language.unique,
										(language) => html`
											<uui-combobox-list-option role="option" value="${language.unique}">
												${language.name}
												${this.#isLanguageReadOnly(language.unique)
													? this.#renderReadOnlyTag(language.unique)
													: nothing}
											</uui-combobox-list-option>
										`,
									)}
								</uui-combobox-list>
							`
						: nothing}
				</uui-scroll-container>
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
