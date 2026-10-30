import type {
	UmbDocumentLinkPickerModalData,
	UmbDocumentLinkPickerModalValue,
} from './document-link-picker-modal.token.js';
import { UmbDocumentLinkPickerContext } from './document-link-picker.context.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_TREE_ALIAS } from '@umbraco-cms/backoffice/document';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalInteractionMemoryController } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbPickerSearchFieldElement } from '@umbraco-cms/backoffice/picker';

const EMPTY_VALUE = 'empty_value';

const MODAL_MEMORY_UNIQUE = 'UmbDocumentLinkPickerModal';

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
	private _isSearchable = false;

	@state()
	private _activeTab: 'browse' | 'search' = 'browse';

	@query('umb-picker-search-field')
	private _searchField?: UmbPickerSearchFieldElement;

	#pickerContext = new UmbDocumentLinkPickerContext(this);

	constructor() {
		super();

		new UmbModalInteractionMemoryController(this, {
			memory: this.#pickerContext.interactionMemory,
			unique: MODAL_MEMORY_UNIQUE,
		});

		this.observe(
			this.#pickerContext.languages,
			(languages) => {
				this._languages = languages;
			},
			null,
		);

		this.observe(
			this.#pickerContext.culture,
			(culture) => {
				this._selectedCulture = culture;
			},
			null,
		);

		this.observe(
			this.#pickerContext.selection.hasSelection,
			(hasSelection) => {
				this._hasSelection = hasSelection;
			},
			null,
		);

		this.observe(
			this.#pickerContext.search.searchable,
			(isSearchable) => {
				this._isSearchable = isSearchable ?? false;
			},
			null,
		);
	}

	override connectedCallback(): void {
		super.connectedCallback();
		// Set once connected: the location restores from memory, which the interaction memory controller only bridges
		// in on connect.
		this.#pickerContext.location.setTreeAlias(UMB_DOCUMENT_TREE_ALIAS);
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

	async #setActiveTab(tab: 'browse' | 'search') {
		if (this._activeTab === tab) return;
		this._activeTab = tab;

		if (tab === 'search') {
			await this.updateComplete;
			this._searchField?.focus();
		}
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('general_choose')} main-no-padding>
				${this.#renderTabs()} ${this.#renderLanguageSelector()}
				<div id="browse" ?hidden=${this._activeTab !== 'browse'}>
					<umb-document-picker-browse hide-tree-root></umb-document-picker-browse>
				</div>
				<div id="search" ?hidden=${this._activeTab !== 'search'}>${this.#renderSearch()}</div>
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

	#renderTabs() {
		if (!this._isSearchable) return nothing;

		return html`
			<uui-tab-group slot="navigation">
				${this.#renderTab('browse', 'picker_browseTab', 'icon-list')}
				${this.#renderTab('search', 'picker_searchTab', 'icon-search')}
			</uui-tab-group>
		`;
	}

	// The label is passed as both property and child text: `uui-tab` renders its `label` in the default
	// slot, which the whitespace of a multi-line template would otherwise occupy.
	#renderTab(tab: 'browse' | 'search', labelKey: string, icon: string) {
		const label = this.localize.term(labelKey);

		return html`<uui-tab
			label=${label}
			?active=${this._activeTab === tab}
			@click=${() => this.#setActiveTab(tab)}
			data-mark="picker:tab:${tab}">
			<umb-icon slot="icon" name=${icon}></umb-icon>
			${label}
		</uui-tab>`;
	}

	#renderSearch() {
		if (!this._isSearchable) return nothing;

		return html`
			<umb-picker-search-field .alias=${UMB_DOCUMENT_TREE_ALIAS}></umb-picker-search-field>
			<umb-picker-search-result></umb-picker-search-result>
		`;
	}

	#renderLanguageSelector() {
		if (!this.data?.allowCultureSpecificLinks) return nothing;
		if (this._languages.length <= 1) return nothing;

		const value = this._selectedCulture || EMPTY_VALUE;

		return html`
			<div id="language">
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
			</div>
		`;
	}

	async #onLanguageSelectionChange(event: Event) {
		const target = event.target as any;
		const value = target?.value as string;
		await this.#pickerContext.setCulture(!value || value === EMPTY_VALUE ? null : value);
	}

	static override styles = [
		css`
			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}

			#language {
				padding: var(--uui-size-layout-1) var(--uui-size-layout-1) 0;
			}

			uui-combobox {
				width: 100%;
			}

			#browse {
				padding-top: var(--uui-size-layout-1);
			}

			#search {
				padding: var(--uui-size-layout-1);
			}

			#language ~ #browse,
			#language ~ #search {
				padding-top: var(--uui-size-space-3);
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
