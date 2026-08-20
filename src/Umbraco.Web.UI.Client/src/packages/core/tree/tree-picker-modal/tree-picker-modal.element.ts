import { UmbTreeItemPickerContext, type UmbTreeItemPickerLocation } from '../tree-item-picker/index.js';
import type { UmbTreeElement } from '../tree.element.js';
import type { UmbTreeItemModelBase, UmbTreeSelectionConfiguration, UmbTreeStartNode } from '../types.js';
import { UmbTreeItemOpenEvent } from '../tree-item/events/tree-item-open.event.js';
import '../components/tree-item-picker-breadcrumb/index.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './types.js';
import { css, customElement, html, ifDefined, nothing, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbPickerModalBaseElement, type UmbPickerSearchFieldElement } from '@umbraco-cms/backoffice/picker';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityExpansionModel, UmbExpansionChangeEvent } from '@umbraco-cms/backoffice/utils';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

const TREE_MEMORY_UNIQUE = 'UmbTreeItemPickerTree';

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
	private _selectionCount: number = 0;

	@state()
	private _createPath?: string;

	@state()
	private _createLabel?: string;

	@state()
	private _isSearchable: boolean = false;

	@state()
	private _activeTab: 'browse' | 'search' = 'browse';

	@state()
	private _treeExpansion: UmbEntityExpansionModel = [];

	@state()
	private _treeInteractionMemories?: Array<UmbInteractionMemoryModel>;

	/** Tri-state, as the location manager reports it: undefined until established, null for a node the tree does not have. */
	@state()
	private _currentLocation?: UmbTreeItemPickerLocation | null;

	@state()
	private _treeStartNode?: UmbTreeStartNode;

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
		this.#observeExpansion();
		this.#observeTreeInteractionMemories();
		this.#observeLocation();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#initCreateAction();
		this.addEventListener(UmbTreeItemOpenEvent.TYPE, this.#onTreeItemOpen);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.removeEventListener(UmbTreeItemOpenEvent.TYPE, this.#onTreeItemOpen);
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

			if (this.data?.treeAlias && this.data.treeAlias !== this._treeAlias) {
				this._treeAlias = this.data.treeAlias;
				this._pickerContext.location.setStartNode(this.data.startNode);
				this._pickerContext.location.setTreeAlias(this.data.treeAlias);
			}

			if (this.data?.treeExpansion !== undefined) {
				this._pickerContext.expansion.setExpansion(this.data.treeExpansion);
			}
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

	#onTreeItemOpen = async (event: UmbTreeItemOpenEvent) => {
		event.stopPropagation();
		const { unique, entityType } = event;
		await this._pickerContext.location.navigateTo({ unique, entityType });
	};

	#observeLocation() {
		this.observe(
			this._pickerContext.location.currentLocation,
			(location) => {
				this._currentLocation = location;
				this._treeStartNode = location?.unique
					? { unique: location.unique, entityType: location.entityType }
					: undefined;
			},
			'umbTreeItemPickerLocationObserver',
		);
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

	#observeExpansion() {
		this.observe(
			this._pickerContext.expansion.expansion,
			(value) => {
				this._treeExpansion = value;
			},
			'umbTreeItemPickerExpansionObserver',
		);
	}

	#observeTreeInteractionMemories() {
		this.observe(
			this._pickerContext.interactionMemory.memory(TREE_MEMORY_UNIQUE),
			(memory) => {
				this._treeInteractionMemories = memory?.memories;
			},
			'umbTreePickerInteractionMemoriesObserver',
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
						this.value = { selection: [value.unique] };
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

	#onTreeInteractionMemoriesChange(event: Event) {
		event.stopPropagation();
		const tree = event.currentTarget as UmbTreeElement;
		const memories = tree.interactionMemories;
		if (memories.length > 0) {
			this._pickerContext.interactionMemory.setMemory({ unique: TREE_MEMORY_UNIQUE, memories });
		} else {
			this._pickerContext.interactionMemory.deleteMemory(TREE_MEMORY_UNIQUE);
		}
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
		return html`
			<umb-body-layout headline=${this.localize.string(this.data?.headline ?? '#general_choose')}>
				${this.#renderTabs()}
				<div id="browse" ?hidden=${this._activeTab !== 'browse'}>${this.#renderTree()}</div>
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
			<umb-picker-search-field .alias=${this._treeAlias}></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${selectableFilter}></umb-picker-search-result>
		`;
	}

	#renderTree() {
		// A level the tree cannot describe has no renderer, so the tree does not stand in for it.
		if (this._currentLocation === null) {
			return html`${this.#renderBreadcrumb()}
				<div id="not-found" class="uui-text">
					<h4>${this.localize.term('general_notFound')}</h4>
				</div>`;
		}

		return html`
			${this.#renderBreadcrumb()}
			<umb-tree
				alias=${ifDefined(this.data?.treeAlias)}
				.props=${{
					showToolbar: true,
					hideTreeItemActions: true,
					hideTreeRoot: this.data?.hideTreeRoot,
					expandTreeRoot: this.data?.expandTreeRoot,
					selectionConfiguration: this._selectionConfiguration,
					filter: this.data?.filter,
					selectableFilter: this.data?.pickableFilter,
					startNode: this._treeStartNode,
					foldersOnly: this.data?.foldersOnly,
					expansion: this._treeExpansion,
					interactionMemories: this._treeInteractionMemories,
				}}
				@selected=${this.#onTreeItemSelected}
				@deselected=${this.#onTreeItemDeselected}
				@expansion-change=${this.#onTreeItemExpansionChange}
				@interaction-memories-change=${this.#onTreeInteractionMemoriesChange}></umb-tree>
		`;
	}

	#renderBreadcrumb() {
		return html`<umb-tree-item-picker-breadcrumb id="breadcrumb"></umb-tree-item-picker-breadcrumb>`;
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
				${this._createPath
					? html` <uui-button
							label=${this.localize.string(this._createLabel ?? '#general_create')}
							look="secondary"
							href=${this._createPath}></uui-button>`
					: nothing}
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

		#breadcrumb {
			margin-bottom: var(--uui-size-space-4);
		}

		#not-found {
			text-align: center;
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

export default UmbTreePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-picker-modal': UmbTreePickerModalElement<UmbTreeItemModelBase>;
	}
}
