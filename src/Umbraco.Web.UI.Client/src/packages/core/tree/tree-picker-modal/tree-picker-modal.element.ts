import { UmbTreeItemPickerContext } from '../tree-item-picker/index.js';
import type { UmbTreeElement } from '../tree.element.js';
import type { UmbTreeItemModelBase, UmbTreeSelectionConfiguration } from '../types.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './types.js';
import {
	customElement,
	html,
	ifDefined,
	nothing,
	state,
	repeat,
	query,
	css,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbPickerModalBaseElement } from '@umbraco-cms/backoffice/picker';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityExpansionModel, UmbExpansionChangeEvent } from '@umbraco-cms/backoffice/utils';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
@customElement('umb-tree-picker-modal')
export class UmbTreePickerModalElement<TreeItemType extends UmbTreeItemModelBase> extends UmbPickerModalBaseElement<
	TreeItemType,
	UmbTreePickerModalData<TreeItemType>,
	UmbTreePickerModalValue
> {
	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@state()
	private _hasSelection: boolean = false;

	@state()
	private _createPath?: string;

	@state()
	private _createLabel?: string;

	@state()
	private _searchQuery?: string;

	@state()
	private _treeExpansion: UmbEntityExpansionModel = [];

	@state()
	private _selectedLanguage?: string;

	@state()
	private _availableLanguages: Array<UmbLanguageDetailModel> = [];

	@state()
	private _isLanguageDropdownOpen = false;

	@query('#language-dropdown-popover')
	private _languagePopoverElement?: UUIPopoverContainerElement;

	protected _pickerContext = new UmbTreeItemPickerContext(this);

	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	#variantContext = new UmbVariantContext(this);

	constructor() {
		super();
		this._pickerContext.selection.setSelectable(true);
		this.observe(this._pickerContext.selection.hasSelection, (hasSelection) => {
			this._hasSelection = hasSelection;
		});
		this.#observePickerSelection();
		this.#observeSearch();
		this.#observeExpansion();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#initCreateAction();
		this.#observeLanguages();
	}

	async #observeLanguages() {
		const { data } = await this.#languageCollectionRepository.requestCollection({});
		if (data) {
			this._availableLanguages = data.items;
		}

		if (this._availableLanguages && this._availableLanguages.length > 0) {
			// Set default/fallback language (first language in the list)
			const defaultLanguage = this._availableLanguages[0];
			await this.#variantContext.setFallbackCulture(defaultLanguage.unique);

			// Set initial display culture from app language context
			const appLanguageContext = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
			if (appLanguageContext) {
				const appCulture = appLanguageContext.getAppCulture();
				if (appCulture) {
					this._selectedLanguage = appCulture;
					await this.#variantContext.setCulture(appCulture);
				} else {
					// If no app culture, use default language
					this._selectedLanguage = defaultLanguage.unique;
					await this.#variantContext.setCulture(defaultLanguage.unique);
				}
			}
		}
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			if (this.data?.search) {
				this._pickerContext.search.updateConfig({
					...this.data.search,
					searchFrom: this.data.startNode,
					dataTypeUnique: this._pickerContext.dataType?.unique,
				});
			}

			const multiple = this.data?.multiple ?? false;
			this._pickerContext.selection.setMultiple(multiple);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				multiple,
			};
		}

		if (_changedProperties.has('value')) {
			const selection = this.value?.selection ?? [];
			this._pickerContext.selection.setSelection(selection);
			this._selectionConfiguration = {
				...this._selectionConfiguration,
				selection: [...selection],
			};
		}
	}

	#observePickerSelection() {
		this.observe(
			this._pickerContext.selection.selection,
			(selection) => {
				this.updateValue({
					selection,
					culture: this._selectedLanguage,
				});
				this.requestUpdate();
			},
			'umbPickerSelectionObserver',
		);
	}

	#observeSearch() {
		this.observe(
			this._pickerContext.search.query,
			(query) => {
				this._searchQuery = query?.query;
			},
			'umbPickerSearchQueryObserver',
		);
	}

	#observeExpansion() {
		this.observe(
			this._pickerContext.expansion.expansion,
			(value) => {
				this._treeExpansion = value;
			},
			'umbTreeItemPickerExpansionObserver',
		);
	}

	// Tree Selection
	#onTreeItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this._pickerContext.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onTreeItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this._pickerContext.selection.deselect(event.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
	}

	// Create action
	#initCreateAction() {
		// TODO: If data.enableCreate is true, we should add a button to create a new item. [NL]
		// Does the tree know enough about this, for us to be able to create a new item? [NL]
		// I think we need to be able to get entityType and a parentId?, or do we only allow creation in the root? and then create via entity actions? [NL]
		// To remove the hardcoded URLs for workspaces of entity types, we could make an create event from the tree, which either this or the sidebar impl. will pick up and react to. [NL]
		// Or maybe the tree item context base can handle this? [NL]
		// Maybe its a general item context problem to be solved. [NL]
		const createAction = this.data?.createAction;
		if (createAction) {
			this._createLabel = createAction.label;
			new UmbModalRouteRegistrationController(
				this,
				(createAction.modalToken as typeof UMB_WORKSPACE_MODAL) ?? UMB_WORKSPACE_MODAL,
			)
				.onSetup(() => {
					return { data: createAction.modalData };
				})
				.onSubmit((value) => {
					if (value) {
						this.value = { selection: [value.unique], culture: this._selectedLanguage };
						this._submitModal();
					} else {
						this._rejectModal();
					}
				})
				.observeRouteBuilder((routeBuilder) => {
					const oldPath = this._createPath;
					this._createPath =
						routeBuilder({}) + createAction.extendWithPathPattern.generateLocal(createAction.extendWithPathParams);
					this.requestUpdate('_createPath', oldPath);
				});
		}
	}

	#onTreeItemExpansionChange(event: UmbExpansionChangeEvent) {
		const target = event.target as UmbTreeElement;
		const expansion = target.getExpansion();
		this._pickerContext.expansion.setExpansion(expansion);
	}

	#searchSelectableFilter = () => true;

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('general_choose')}>
				<uui-box> ${this.#renderSearch()} ${this.#renderTree()}</uui-box>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}
	#renderSearch() {
		const selectableFilter =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter ?? this.#searchSelectableFilter;

		return html`
			${this.#renderLanguageSelector()}
			<umb-picker-search-field></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${selectableFilter}></umb-picker-search-result>
		`;
	}

	#renderLanguageSelector() {
		if (!this.data?.isVariant || this._availableLanguages.length <= 1) {
			return nothing;
		}

		const selectedLanguage = this._availableLanguages.find((lang) => lang.unique === this._selectedLanguage);

		return html`
			<div style="border-bottom: 1px solid var(--uui-color-border);">
				<div id="language-toggle" popovertarget="language-dropdown-popover" @click=${this.#onLanguageTriggerClick}>
					<span>${selectedLanguage?.name || 'Select Language'}</span>
					<uui-symbol-expand .open=${this._isLanguageDropdownOpen}></uui-symbol-expand>
				</div>
				${this.#renderLanguageContent()}
			</div>
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
			<uui-combobox-list
				aria-label="Tree Picker Language"
				.value=${this._selectedLanguage || ''}
				@change=${this.#onLanguageSelectionChange}>
				${repeat(
					this._availableLanguages,
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
		if (value) {
			this._selectedLanguage = value;

			// Update variant context - this will trigger tree items to re-render
			await this.#variantContext.setCulture(value);

			// Force re-render
			this.requestUpdate();
			this._languagePopoverElement?.hidePopover();
		}
	}

	#renderTree() {
		if (this._searchQuery) {
			return nothing;
		}

		return html`
			<umb-tree
				alias=${ifDefined(this.data?.treeAlias)}
				.props=${{
					hideTreeItemActions: true,
					hideTreeRoot: this.data?.hideTreeRoot,
					expandTreeRoot: this.data?.expandTreeRoot,
					selectionConfiguration: this._selectionConfiguration,
					filter: this.data?.filter,
					selectableFilter: this.data?.pickableFilter,
					startNode: this.data?.startNode,
					foldersOnly: this.data?.foldersOnly,
					expansion: this._treeExpansion,
				}}
				@selected=${this.#onTreeItemSelected}
				@deselected=${this.#onTreeItemDeselected}
				@expansion-change=${this.#onTreeItemExpansionChange}></umb-tree>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				${this._createPath
					? html` <uui-button
							label=${this.localize.string(this._createLabel ?? '#general_create')}
							look="secondary"
							href=${this._createPath}></uui-button>`
					: nothing}
				<uui-button
					label=${this.localize.term('general_choose')}
					look="primary"
					color="positive"
					@click=${this._submitModal}
					?disabled=${!this._hasSelection}></uui-button>
			</div>
		`;
	}

	static override styles = [
		css`
			#language-toggle {
				color: var(--uui-color-text);
				text-align: left;
				background: none;
				border: none;
				height: 40px;
				padding: 0 var(--uui-size-8);
				border-bottom: 1px solid var(--uui-color-border);
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

export default UmbTreePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-picker-modal': UmbTreePickerModalElement<UmbTreeItemModelBase>;
	}
}
