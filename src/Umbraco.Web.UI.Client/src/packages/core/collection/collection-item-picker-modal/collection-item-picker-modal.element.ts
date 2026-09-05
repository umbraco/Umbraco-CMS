import type { UmbCollectionLayoutConfiguration, UmbCollectionSelectionConfiguration } from '../types.js';
import { UmbCollectionItemPickerContext } from './collection-item-picker-modal.context.js';
import type { UmbCollectionItemPickerModalData, UmbCollectionItemPickerModalValue } from './types.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, nothing, ifDefined, css, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbPickerSearchFieldElement } from '@umbraco-cms/backoffice/picker';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-collection-item-picker-modal')
export class UmbCollectionItemPickerModalElement extends UmbModalBaseElement<
	UmbCollectionItemPickerModalData,
	UmbCollectionItemPickerModalValue
> {
	@state()
	private _selectionConfiguration: UmbCollectionSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selectOnly: true,
		selection: [],
	};

	@state()
	private _hasSelection: boolean = false;

	@state()
	private _selectionCount: number = 0;

	@state()
	private _isSearchable: boolean = false;

	@state()
	private _activeTab: 'browse' | 'search' = 'browse';

	@query('umb-picker-search-field')
	private _searchField?: UmbPickerSearchFieldElement;

	#pickerContext = new UmbCollectionItemPickerContext(this);

	constructor() {
		super();
		this.#pickerContext.selection.setSelectable(true);
		this.observe(
			this.#pickerContext.selection.hasSelection,
			(hasSelection) => {
				this._hasSelection = hasSelection;
			},
			null,
		);
		this.#observePickerSelection();
		this.#observeSearch();
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			if (this.data?.search) {
				this.#pickerContext.search.updateConfig({
					...this.data.search,
				});
			}

			const multiple = this.data?.multiple ?? false;
			this.#pickerContext.selection.setMultiple(multiple);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				multiple,
				selectableFilter: this.data?.pickableFilter,
			};
		}

		if (_changedProperties.has('value')) {
			const selection = this.value?.selection ?? [];
			this.#pickerContext.selection.setSelection(selection);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				selection: [...selection].filter((x) => x !== null),
			};
		}
	}

	#observePickerSelection() {
		this.observe(
			this.#pickerContext.selection.selection,
			(selection) => {
				this._selectionCount = selection.length;
				this.updateValue({ selection });
				this.requestUpdate();
			},
			'umbPickerSelectionObserver',
		);
	}

	#observeSearch() {
		this.observe(
			this.#pickerContext?.search.searchable,
			(isSearchable) => (this._isSearchable = isSearchable ?? false),
			null,
		);
	}

	#onItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.deselect(event.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
	}

	#searchSelectableFilter = () => true;

	async #setActiveTab(tab: 'browse' | 'search') {
		if (this._activeTab === tab) return;
		this._activeTab = tab;

		if (tab === 'search') {
			await this.updateComplete;
			this._searchField?.focus();
		}
	}

	override render() {
		const renderCollection = !!this.data?.collection.alias;

		return html`
			<umb-body-layout headline="${this.localize.term('general_choose')}" ?main-no-padding=${renderCollection}>
				${this.#renderTabs()}
				<div id="browse" ?hidden=${this._activeTab !== 'browse'}>${this.#renderMain(renderCollection)}</div>
				<div id="search" ?hidden=${this._activeTab !== 'search'}>${this.#renderSearch()}</div>
				${this.#renderSelectionCount()} ${this.#renderActions()}
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

		const selectableFilter =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter ?? this.#searchSelectableFilter;

		return html`
			<umb-picker-search-field .alias=${this.data?.collection.alias}></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${selectableFilter}></umb-picker-search-result>
		`;
	}

	#renderMain(hasCollectionAlias: boolean) {
		return html` ${hasCollectionAlias ? this.#renderCollection() : this.#renderCollectionMenu()} `;
	}

	#renderCollection() {
		const layouts: Array<UmbCollectionLayoutConfiguration> =
			this.data?.collection.views?.map((view) => {
				return {
					collectionView: view.alias,
				};
			}) ?? [];

		return html`
			<umb-collection
				alias=${ifDefined(this.data?.collection.alias)}
				.config=${{
					layouts,
					selectionConfiguration: this._selectionConfiguration,
					bulkActionConfiguration: { enabled: false },
				}}
				@selected=${this.#onItemSelected}
				@deselected=${this.#onItemDeselected}></umb-collection>
		`;
	}

	#renderCollectionMenu() {
		return html`
			<uui-box id="collection-menu-box"
				><umb-collection-menu
					alias=${ifDefined(this.data?.collection?.menuAlias)}
					.props=${{
						selectionConfiguration: this._selectionConfiguration,
						filterArgs: this.data?.collection?.filterArgs,
						filter: this.data?.filter,
						selectableFilter: this.data?.pickableFilter,
					}}
					@selected=${this.#onItemSelected}
					@deselected=${this.#onItemDeselected}></umb-collection-menu
			></uui-box>
		`;
	}

	#renderSelectionCount() {
		if (!this._selectionConfiguration.multiple || !this._hasSelection) return nothing;

		return html`
			<div id="selection-info" slot="footer">${this.localize.term('picker_selectedCount', this._selectionCount)}</div>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
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
			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
			}

			umb-collection {
				display: block;
				height: fit-content;
			}

			umb-body-layout[main-no-padding] #search {
				padding: var(--uui-size-layout-1);
			}

			#selection-info {
				display: flex;
				align-items: center;
				box-sizing: border-box;
				width: 100%;
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}
		`,
	];
}

export { UmbCollectionItemPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-item-picker-modal': UmbCollectionItemPickerModalElement;
	}
}
