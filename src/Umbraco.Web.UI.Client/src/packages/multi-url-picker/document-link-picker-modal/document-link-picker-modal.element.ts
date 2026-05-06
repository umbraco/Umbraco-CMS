import type {
	UmbDocumentLinkPickerModalData,
	UmbDocumentLinkPickerModalValue,
} from './document-link-picker-modal.token.js';
import { UmbDocumentLinkPickerContext } from './document-link-picker.context.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_TREE_ALIAS } from '@umbraco-cms/backoffice/document';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
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
	private _hasSelection = false;

	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	#pickerContext = new UmbDocumentLinkPickerContext(this);

	constructor() {
		super();

		this.observe(this.#pickerContext.languages, (languages) => {
			this._languages = languages;
		});

		this.observe(this.#pickerContext.culture, (culture) => {
			this._selectedCulture = culture;
		});

		this.observe(this.#pickerContext.selection.hasSelection, (hasSelection) => {
			this._hasSelection = hasSelection;
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
		if (!selectedItemUnique) return;

		this.value = {
			unique: selectedItemUnique,
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			culture: (await this.#pickerContext.getCulture()) || undefined,
		};

		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('general_choose')}>
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
						?disabled=${!this._hasSelection}
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
		if (!this.data?.allowCultureSpecificLinks) return nothing;
		if (this._languages.length <= 1) return nothing;

		const value = this._selectedCulture || EMPTY_VALUE;

		return html`
			<uui-combobox
				placeholder=${this.localize.term('linkPicker_selectLanguageHint')}
				.value=${value}
				@change=${this.#onLanguageSelectionChange}>
				<uui-combobox-list>
					<uui-combobox-list-option value=${EMPTY_VALUE}
						>${this.localize.term('linkPicker_selectLanguageDefault')}</uui-combobox-list-option
					>
					${repeat(
						this._languages,
						(language) => language.unique,
						(language) => html`
							<uui-combobox-list-option value=${language.unique}> ${language.name} </uui-combobox-list-option>
						`,
					)}
				</uui-combobox-list>
			</uui-combobox>
		`;
	}

	async #onLanguageSelectionChange(event: Event) {
		const target = event.target as any;
		const value = target?.value as string;
		await this.#pickerContext.setCulture(!value || value === EMPTY_VALUE ? null : value);
	}

	static override styles = [
		css`
			uui-combobox {
				width: 100%;
				margin-bottom: var(--uui-size-space-3);
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
