import type {
	UmbDocumentLinkPickerModalData,
	UmbDocumentLinkPickerModalValue,
} from './document-link-picker-modal.token.js';
import { UmbDocumentLinkPickerContext } from './document-link-picker.context.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_TREE_ALIAS } from '@umbraco-cms/backoffice/document';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbTreeSelectionConfiguration } from '@umbraco-cms/backoffice/tree';

const EMPTY_VALUE = 'empty_value';

@customElement('umb-document-link-picker-modal')
export class UmbDocumentLinkPickerModalElement extends UmbModalBaseElement<
	UmbDocumentLinkPickerModalData,
	UmbDocumentLinkPickerModalValue
> {
	@state()
	private _languages: Array<UmbLanguageDetailModel> = [];

	@state()
	private _selectedCulture?: string | null;

	@state()
	private _isLanguageDropdownOpen = false;

	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@query('#language-dropdown-popover')
	private _languagePopoverElement?: UUIPopoverContainerElement;

	#pickerContext = new UmbDocumentLinkPickerContext(this);

	constructor() {
		super();

		this.observe(this.#pickerContext.languages, (languages) => {
			this._languages = languages;
		});

		this.observe(this.#pickerContext.culture, (culture) => {
			this._selectedCulture = culture;
		});
	}

	// Tree Selection
	#onTreeItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.select(event.unique);
	}

	#onTreeItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.deselect(event.unique);
	}

	async #onSubmitModal() {
		const selectedItemUnique = this.#pickerContext.selection.getSelection()[0];
		if (!selectedItemUnique) {
			throw new Error('No item selected');
		}

		this.value = {
			unique: selectedItemUnique,
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			culture: await this.#pickerContext.getCulture(),
		};

		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline="Pick Document">
				${this.#renderLanguageSelector()} ${this.#renderSearch()}
				<uui-box>
					<umb-tree
						alias=${UMB_DOCUMENT_TREE_ALIAS}
						.props=${{
							hideTreeItemActions: true,
							hideTreeRoot: true,
							selectionConfiguration: this._selectionConfiguration,
						}}
						@selected=${this.#onTreeItemSelected}
						@deselected=${this.#onTreeItemDeselected}></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						color="positive"
						look="primary"
						label=${this.localize.term('general_choose')}
						@click=${this.#onSubmitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderSearch() {
		return html`
			<umb-picker-search-field></umb-picker-search-field>
			<umb-picker-search-result></umb-picker-search-result>
		`;
	}

	#renderLanguageSelector() {
		if (this._languages.length === 1) return nothing;

		const selectedLanguage = this._languages.find((lang) => lang.unique === this._selectedCulture);

		return html`
			<div id="language-toggle" popovertarget="language-dropdown-popover" @click=${this.#onLanguageTriggerClick}>
				<span>${selectedLanguage?.name || 'Viewer’s language'}</span>
				<uui-symbol-expand .open=${this._isLanguageDropdownOpen}></uui-symbol-expand>
			</div>
			${this.#renderLanguageContent()}
		`;
	}

	#renderLanguageContent() {
		return html`
			<uui-popover-container
				id="language-dropdown-popover"
				@beforetoggle=${this.#onLanguageBeforePopoverToggle}
				@toggle=${this.#onLanguagePopoverToggle}>
				<div id="dropdown">
					<uui-scroll-container> ${this.#renderLanguageOptions()} </uui-scroll-container>
				</div>
			</uui-popover-container>
		`;
	}

	#renderLanguageOptions() {
		if (!this._isLanguageDropdownOpen) return nothing;

		return html`
			<uui-combobox-list .value=${this._selectedCulture || ''} @change=${this.#onLanguageSelectionChange}>
				<uui-combobox-list-option value=${EMPTY_VALUE}>Viewer’s language</uui-combobox-list-option>
				${repeat(
					this._languages,
					(language) => language.unique,
					(language) => html`
						<uui-combobox-list-option value=${language.unique}> ${language.name} </uui-combobox-list-option>
					`,
				)}
			</uui-combobox-list>
		`;
	}

	#onLanguageTriggerClick() {
		if (this._isLanguageDropdownOpen) {
			this._languagePopoverElement?.hidePopover();
		} else {
			this._languagePopoverElement?.showPopover();
		}
		this.requestUpdate();
	}

	#onLanguageBeforePopoverToggle(event: ToggleEvent) {
		if (event.newState === 'open' && this._languagePopoverElement) {
			const host = this.getBoundingClientRect();
			this._languagePopoverElement.style.width = `${host.width - 80}px`;
		}
	}

	#onLanguagePopoverToggle(event: ToggleEvent) {
		this._isLanguageDropdownOpen = event.newState === 'open';
	}

	async #onLanguageSelectionChange(event: Event) {
		const target = event.target as any;
		const value = target?.value as string;
		this.#pickerContext.setCulture(value === EMPTY_VALUE ? null : value);
		this._languagePopoverElement?.hidePopover();
	}

	static override styles = [
		css`
			uui-combobox {
				width: 100%;
			}

			#language-toggle {
				color: var(--uui-color-text);
				text-align: left;
				background: none;
				border: none;
				height: 40px;
				padding: 0 var(--uui-size-8);
				margin-bottom: var(--uui-size-4);
				font-size: 14px;
				display: flex;
				align-items: center;
				justify-content: space-between;
				cursor: pointer;
				font-family: inherit;
				box-sizing: border-box;
				width: 100%;
			}

			#language-toggle:hover {
				background-color: var(--uui-color-surface-emphasis);
			}

			#language-dropdown-popover uui-scroll-container {
				max-height: 200px;
			}

			uui-combobox-list-option {
				padding: var(--uui-size-2) var(--uui-size-4);
				display: flex;
				align-items: center;
				font-size: 14px;
			}

			#dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: auto;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
			}
		`,
	];
}

export { UmbDocumentLinkPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-link-picker-modal': UmbDocumentLinkPickerModalElement;
	}
}
