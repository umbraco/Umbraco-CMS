import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import type { UmbDocumentPickerModalData, UmbDocumentPickerModalValue } from './types.js';
import { UMB_CONTENT_SECTION_ALIAS, UmbContentCollectionConfigurationContext } from '@umbraco-cms/backoffice/content';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import {
	css,
	customElement,
	html,
	ifDefined,
	keyed,
	nothing,
	query,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbPickerModalBaseElement, type UmbPickerSearchFieldElement } from '@umbraco-cms/backoffice/picker';
import { UmbTreeItemOpenEvent, UmbTreeItemPickerContext } from '@umbraco-cms/backoffice/tree';
import type {
	UmbTreeElement,
	UmbTreeItemPickerLocation,
	UmbTreeSelectionConfiguration,
	UmbTreeStartNode,
} from '@umbraco-cms/backoffice/tree';
import type {
	UmbCollectionConfiguration,
	UmbCollectionElement,
	UmbCollectionItemModel,
	UmbCollectionSelectionConfiguration,
} from '@umbraco-cms/backoffice/collection';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityExpansionModel, UmbExpansionChangeEvent } from '@umbraco-cms/backoffice/utils';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

const TREE_MEMORY_UNIQUE = 'UmbTreeItemPickerTree';
const COLLECTION_MEMORY_UNIQUE = 'UmbItemPickerCollection';
const ROOT_MEMORY_KEY = 'root';

/**
 * The part of a content tree item this modal reads to decide how a node's children render. A tree that does not map
 * `contentType` is browsed as a tree throughout.
 */
type UmbContentTreeItemLike = {
	contentType?: {
		collection?: { unique: string } | null;
	};
};

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
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@state()
	private _collectionSelectionConfiguration: UmbCollectionSelectionConfiguration = {
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

	@state()
	private _treeExpansion: UmbEntityExpansionModel = [];

	@state()
	private _treeInteractionMemories?: Array<UmbInteractionMemoryModel>;

	@state()
	private _collectionInteractionMemories?: Array<UmbInteractionMemoryModel>;

	/** Tri-state, as the location manager reports it: undefined until established, null for a node the tree does not have. */
	@state()
	private _currentLocation?: UmbTreeItemPickerLocation | null;

	@state()
	private _treeStartNode?: UmbTreeStartNode;

	@state()
	private _hasCollection = false;

	@state()
	private _collectionConfig?: UmbCollectionConfiguration;

	@state()
	private _hasContentSectionAccess = false;

	@query('umb-picker-search-field')
	private _searchField?: UmbPickerSearchFieldElement;

	@state()
	private _treeAlias?: string;

	/**
	 * Answers `UMB_ENTITY_CONTEXT` for the node being browsed. Without it the collection would bind to the entity of
	 * whichever element opened the modal, because a modal re-dispatches unanswered context requests onto its opener.
	 */
	#entityContext = new UmbEntityContext(this);

	#collectionConfiguration = new UmbContentCollectionConfigurationContext(this);

	#collectionMemories: Array<UmbInteractionMemoryModel> = [];

	protected _pickerContext = new UmbTreeItemPickerContext(this);

	constructor() {
		super();
		// Which collection this picker renders follows from it being a document picker, so it is not a caller's choice.
		this.#collectionConfiguration.setCollectionAlias(UMB_DOCUMENT_COLLECTION_ALIAS);
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
		this.#observeCollectionInteractionMemories();
		this.#observeCollectionConfiguration();
		this.#observeContentSectionAccess();
		this.#observeLocation();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.addEventListener(UmbTreeItemOpenEvent.TYPE, this.#onItemOpen);
	}

	override disconnectedCallback(): void {
		super.disconnectedCallback();
		this.removeEventListener(UmbTreeItemOpenEvent.TYPE, this.#onItemOpen);
	}

	protected override willUpdate(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.willUpdate(_changedProperties);

		// The single point where a location is turned into a rendering decision. Everything that browses — a tree item
		// opening, a collection item drilling in, a breadcrumb click, a restore from memory — moves `_currentLocation`
		// and nothing else, so the renderer, the entity context and the configuration cannot drift apart.
		if (_changedProperties.has('_currentLocation')) {
			this.#resolveLocation();
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

			this._collectionSelectionConfiguration = {
				...this._collectionSelectionConfiguration,
				multiple,
				selectableFilter: this.data?.pickableFilter as ((item: UmbCollectionItemModel) => boolean) | undefined,
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
			this._collectionSelectionConfiguration = {
				...this._collectionSelectionConfiguration,
				selection: [...selection].filter((unique) => unique !== null),
			};
		}
	}

	#onItemOpen = async (event: UmbTreeItemOpenEvent) => {
		event.stopPropagation();
		const { unique, entityType } = event;
		await this._pickerContext.location.navigateTo({ unique, entityType });
	};

	#getCollectionUnique(location?: UmbTreeItemPickerLocation | null): string | undefined {
		return (location as (UmbTreeItemPickerLocation & UmbContentTreeItemLike) | undefined)?.contentType?.collection
			?.unique;
	}

	/**
	 * Points the entity context and the collection configuration at the node being browsed, and picks up that node's
	 * remembered collection state. The root renders the tree, as it has no content type and therefore no collection.
	 */
	#resolveLocation() {
		const location = this._currentLocation;

		this.#entityContext.setEntityType(location?.entityType);
		this.#entityContext.setUnique(location?.unique ?? null);

		this.#collectionConfiguration.setUnique(location?.unique ?? null);
		this.#collectionConfiguration.setDataTypeUnique(this.#getCollectionUnique(location));

		this.#updateCollectionInteractionMemories();
	}

	#observeLocation() {
		this.observe(
			this._pickerContext.location.currentLocation,
			(location) => {
				this._currentLocation = location;
				this._treeStartNode = location?.unique
					? { unique: location.unique, entityType: location.entityType }
					: undefined;
			},
			'umbContentPickerLocationObserver',
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

	#observeCollectionInteractionMemories() {
		this.observe(
			this._pickerContext.interactionMemory.memory(COLLECTION_MEMORY_UNIQUE),
			(memory) => {
				this.#collectionMemories = memory?.memories ?? [];
				this.#updateCollectionInteractionMemories();
			},
			'umbContentPickerCollectionInteractionMemoriesObserver',
		);
	}

	#observeCollectionConfiguration() {
		this.observe(
			this.#collectionConfiguration.hasCollection,
			(hasCollection) => {
				this._hasCollection = hasCollection;
			},
			null,
		);

		this.observe(
			this.#collectionConfiguration.collectionConfig,
			(collectionConfig) => {
				this._collectionConfig = collectionConfig;
			},
			null,
		);
	}

	/** The Collection endpoint requires Content-section access; the Tree endpoint accepts any section. */
	#observeContentSectionAccess() {
		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.allowedSections,
				(allowedSections) => {
					this._hasContentSectionAccess = allowedSections?.includes(UMB_CONTENT_SECTION_ALIAS) ?? false;
				},
				'umbDocumentPickerContentSectionAccessObserver',
			);
		});
	}

	/**
	 * Collection memories are nested per browsed node, so returning to a node restores the view, filter, ordering and
	 * page it was left in while a sibling starts clean. An ordering column configured on one content type need not
	 * exist on another.
	 * @returns {string} The memory key of the node being browsed.
	 */
	#getCollectionMemoryKey(): string {
		return this._currentLocation?.unique ?? ROOT_MEMORY_KEY;
	}

	#updateCollectionInteractionMemories() {
		const key = this.#getCollectionMemoryKey();
		this._collectionInteractionMemories = this.#collectionMemories.find((memory) => memory.unique === key)?.memories;
	}

	#onCollectionInteractionMemoriesChange(event: Event) {
		event.stopPropagation();
		const collection = event.currentTarget as UmbCollectionElement;
		const memories = collection.getInteractionMemories();
		const key = this.#getCollectionMemoryKey();

		const others = this.#collectionMemories.filter((memory) => memory.unique !== key);
		const next = memories.length ? [...others, { unique: key, memories }] : others;

		if (next.length) {
			this._pickerContext.interactionMemory.setMemory({ unique: COLLECTION_MEMORY_UNIQUE, memories: next });
		} else {
			this._pickerContext.interactionMemory.deleteMemory(COLLECTION_MEMORY_UNIQUE);
		}
	}

	#onItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this._pickerContext.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this._pickerContext.selection.deselect(event.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
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

	/** Whether the collection is actually being rendered for the browsed node, rather than merely configured for it. */
	#willRenderCollection(): boolean {
		return this._hasCollection && this._hasContentSectionAccess;
	}

	override render() {
		return html`
			<umb-body-layout
				headline=${this.localize.string(this.data?.headline ?? '#general_choose')}
				?main-no-padding=${this.#willRenderCollection()}>
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

		const selectableFilter =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter ?? this.#searchSelectableFilter;

		return html`
			<umb-picker-search-field .alias=${this._treeAlias}></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${selectableFilter}></umb-picker-search-result>
		`;
	}

	#renderBrowse() {
		return html`${this.#renderBreadcrumb()} ${this.#renderChildren()}`;
	}

	#renderChildren() {
		// Which renderer a level needs is only known once the location is, and mounting the tree in the meantime would
		// tear it down again mid-initialisation.
		if (this._currentLocation === undefined) return html`<umb-view-loader></umb-view-loader>`;
		// A level the tree cannot describe has no renderer, so neither the tree nor the collection stands in for it.
		if (this._currentLocation === null) return this.#renderNotFound();
		if (!this.#willRenderCollection()) return this.#renderTree();
		// The configuration is resolved from a data type, so the collection is held back rather than briefly showing
		// the tree in its place.
		return this._collectionConfig ? this.#renderCollection() : html`<umb-view-loader></umb-view-loader>`;
	}

	#renderNotFound() {
		return html`<div id="not-found" class="uui-text">
			<h4>${this.localize.term('general_notFound')}</h4>
		</div>`;
	}

	#renderTree() {
		return html`
			<umb-tree
				alias=${ifDefined(this.data?.treeAlias)}
				.props=${{
					showToolbar: true,
					drillable: true,
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
				@selected=${this.#onItemSelected}
				@deselected=${this.#onItemDeselected}
				@expansion-change=${this.#onTreeItemExpansionChange}
				@interaction-memories-change=${this.#onTreeInteractionMemoriesChange}></umb-tree>
		`;
	}

	#renderCollection() {
		const config: UmbCollectionConfiguration = {
			...this._collectionConfig,
			selectionConfiguration: this._collectionSelectionConfiguration,
			bulkActionConfiguration: { enabled: false },
			hideItemActions: true,
			hideCollectionActions: true,
		};

		// Re-created per browsed node: a collection cannot be retargeted once configured, and a fresh instance also
		// resets pagination and the per-view registrations.
		return keyed(
			this.#getCollectionMemoryKey(),
			html`<umb-collection
				alias=${UMB_DOCUMENT_COLLECTION_ALIAS}
				.config=${config}
				.interactionMemories=${this._collectionInteractionMemories}
				@selected=${this.#onItemSelected}
				@deselected=${this.#onItemDeselected}
				@interaction-memories-change=${this.#onCollectionInteractionMemoriesChange}></umb-collection>`,
		);
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

		umb-body-layout[main-no-padding] #search {
			padding: var(--uui-size-layout-1);
		}

		#breadcrumb {
			margin-bottom: var(--uui-size-space-4);
		}

		#not-found {
			text-align: center;
			padding: var(--uui-size-layout-1);
		}

		/* A collection lays itself out inside its own body layout, so the main padding is dropped for it and only the
		   breadcrumb above it is inset — padding the whole panel would inset the collection twice. */
		umb-body-layout[main-no-padding] #breadcrumb {
			padding: var(--uui-size-layout-1) var(--uui-size-layout-1) 0;
			margin-bottom: 0;
		}

		uui-breadcrumbs {
			overflow: hidden;
			min-width: 0;
		}

		uui-breadcrumb-item:not([last-item]) {
			cursor: pointer;
		}

		umb-collection {
			display: block;
			height: fit-content;
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
