import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/constants.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from './types.js';
import { css, customElement, html, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbPickerModalBaseElement, type UmbPickerSearchFieldElement } from '@umbraco-cms/backoffice/picker';
import { UmbTreeItemPickerContext } from '@umbraco-cms/backoffice/tree';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

import '../../components/document-picker-browse/document-picker-browse.element.js';

/**
 * A picker that browses documents, rendering the document collection at any level whose node has one configured
 * (and the user has Content-section access) and the tree everywhere else.
 * @element umb-document-picker-modal
 */
@customElement('umb-document-picker-modal')
export class UmbDocumentPickerModalElement extends UmbPickerModalBaseElement<
	UmbDocumentTreeItemModel,
	UmbDocumentPickerModalData,
	UmbDocumentPickerModalValue
> {
	@state()
	private _multiple = false;

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

	@state()
	private _treeAlias?: string;

	protected _pickerContext = new UmbTreeItemPickerContext(this);

	constructor() {
		super();
		this._pickerContext.selection.setSelectable(true);
		this.observe(
			this._pickerContext.selection.hasSelection,
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
			this.#warnUnappliedOptions();

			// eslint-disable-next-line @typescript-eslint/no-deprecated
			const treeAlias = this.data?.treeAlias ?? UMB_DOCUMENT_TREE_ALIAS;
			if (treeAlias !== this._treeAlias) {
				this._treeAlias = treeAlias;
				this._pickerContext.location.setStartNode(this.data?.startNode);
				this._pickerContext.location.setStartNodes(this.data?.startNodes);
				this._pickerContext.location.setTreeAlias(treeAlias);
			}

			if (this.data?.search) {
				this._pickerContext.search.updateConfig({
					...this.data.search,
					// There is no server side search across multiple parents, so the scope is applied client side.
					searchFrom: this._pickerContext.location.getStartNode(),
					dataTypeUnique: this._pickerContext.dataType?.unique,
				});
			}

			this._multiple = this.data?.multiple ?? false;
			this._pickerContext.selection.setMultiple(this._multiple);

			if (this.data?.treeExpansion !== undefined) {
				this._pickerContext.expansion.setExpansion(this.data.treeExpansion);
			}
		}

		if (_changedProperties.has('value')) {
			this._pickerContext.selection.setSelection(this.value?.selection ?? []);
		}
	}

	#warnUnappliedOptions() {
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		if (this.data?.foldersOnly) {
			new UmbDeprecation({
				deprecated:
					'The `foldersOnly` option of UmbDocumentPickerModalData is not applied, as documents have no folders.',
				removeInVersion: '21.0.0',
				solution: 'Remove the option.',
			}).warn();
		}

		// eslint-disable-next-line @typescript-eslint/no-deprecated
		if (this.data?.createAction) {
			new UmbDeprecation({
				deprecated: 'The `createAction` option of UmbDocumentPickerModalData is not applied by the document picker.',
				removeInVersion: '21.0.0',
				solution: 'Remove the option.',
			}).warn();
		}
	}

	#observePickerSelection() {
		this.observe(
			this._pickerContext.selection.selection,
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
			this._pickerContext.search.searchable,
			(isSearchable) => {
				this._isSearchable = isSearchable ?? false;
			},
			'umbPickerSearchableObserver',
		);
	}

	#onItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
	}

	// Search cannot be scoped server side to more than one parent, so out-of-scope results are filtered here.
	#searchSelectableFilter = (item: UmbSearchResultItemModel & UmbDocumentTreeItemModel) => {
		const baseFilter: ((item: UmbSearchResultItemModel & UmbDocumentTreeItemModel) => boolean) | undefined =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter;
		if (baseFilter && !baseFilter(item)) return false;
		return this._pickerContext.location.isWithinStartNodes(item);
	};

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
			<umb-body-layout headline=${this.localize.string(this.data?.headline ?? '#general_choose')} main-no-padding>
				${this.#renderTabs()}
				<div id="browse" ?hidden=${this._activeTab !== 'browse'}>${this.#renderBrowse()}</div>
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

		return html`
			<umb-picker-search-field .alias=${this._treeAlias}></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${this.#searchSelectableFilter}></umb-picker-search-result>
		`;
	}

	#renderBrowse() {
		return html`<umb-document-picker-browse
			.pickableFilter=${this.data?.pickableFilter}
			.filter=${this.data?.filter}
			.treeAlias=${this._treeAlias}
			?hide-tree-root=${this.data?.hideTreeRoot}
			?expand-tree-root=${this.data?.expandTreeRoot}
			@selected=${this.#onItemSelected}
			@deselected=${this.#onItemDeselected}></umb-document-picker-browse>`;
	}

	#renderSelectionCount() {
		if (!this._multiple || !this._hasSelection) return nothing;

		return html`
			<div id="selection-info" slot="footer">${this.localize.term('picker_selectedCount', this._selectionCount)}</div>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				<uui-button
					label=${this.localize.string(this.data?.confirmLabel ?? '#general_choose')}
					look="primary"
					color="positive"
					@click=${this._submitModal}
					?disabled=${!this._hasSelection}></uui-button>
			</div>
		`;
	}

	static override styles = css`
		uui-tab-group {
			--uui-tab-divider: var(--uui-color-border);
			border-left: 1px solid var(--uui-color-border);
			border-right: 1px solid var(--uui-color-border);
		}

		#browse {
			padding-top: var(--uui-size-layout-1);
		}

		#search {
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
	`;
}

export default UmbDocumentPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-picker-modal': UmbDocumentPickerModalElement;
	}
}
