import { UmbContentCollectionConfigurationContext } from '../collection/configuration/content-collection-configuration.context.js';
import type { UmbContentPickerModalData, UmbContentPickerModalValue } from './types.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	keyed,
	nothing,
	query,
	repeat,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbPickerContext,
	UmbPickerModalBaseElement,
	type UmbPickerSearchFieldElement,
} from '@umbraco-cms/backoffice/picker';
import { UmbTreeItemOpenEvent, UmbTreeItemPickerExpansionManager } from '@umbraco-cms/backoffice/tree';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestTree,
	UmbTreeElement,
	UmbTreeItemModel,
	UmbTreeItemModelBase,
	UmbTreeRepository,
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
const LOCATION_MEMORY_UNIQUE = 'UmbTreeItemPickerLocation';
const ROOT_MEMORY_KEY = 'root';

/**
 * The node currently being browsed, together with the collection it configures. The two travel as one value so the
 * renderer and the configuration can never disagree about which node they describe.
 */
type UmbContentPickerLocation = UmbTreeStartNode & {
	collectionUnique?: string;
};

interface UmbContentPickerBreadcrumbItem {
	unique: string | null;
	entityType: string;
	name: string;
	collectionUnique?: string;
}

/**
 * The part of a content tree item this modal reads to decide how a node's children render. A content tree that does
 * not map `contentType` is browsed as a tree throughout.
 */
type UmbContentTreeItemLike = {
	contentType?: {
		collection?: { unique: string } | null;
	};
};

/**
 * The same composition as `UmbTreeItemPickerContext`, which cannot be imported here: it is not exported from
 * `@umbraco-cms/backoffice/tree`, and exporting it would put the tree barrel into a cycle with the picker package,
 * which already depends on this one.
 */
class UmbContentPickerContext extends UmbPickerContext {
	public readonly expansion = new UmbTreeItemPickerExpansionManager(this, {
		interactionMemoryManager: this.interactionMemory,
	});
}

/**
 * A picker that browses content, rendering a collection at any level whose node has one configured and the tree
 * everywhere else. The node being browsed decides how its children render, so collection and tree levels interleave.
 * @element umb-content-picker-modal
 */
@customElement('umb-content-picker-modal')
export class UmbContentPickerModalElement<TreeItemType extends UmbTreeItemModelBase> extends UmbPickerModalBaseElement<
	TreeItemType,
	UmbContentPickerModalData<TreeItemType>,
	UmbContentPickerModalValue
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

	@state()
	private _currentLocation?: UmbContentPickerLocation;

	@state()
	private _breadcrumb: Array<UmbContentPickerBreadcrumbItem> = [];

	@state()
	private _hasCollection = false;

	@state()
	private _collectionConfig?: UmbCollectionConfiguration;

	@query('umb-picker-search-field')
	private _searchField?: UmbPickerSearchFieldElement;

	@state()
	private _treeAlias?: string;
	private _initialStartNode?: UmbTreeStartNode;
	private _repository?: UmbTreeRepository;
	private _breadcrumbLoaded = false;
	private _breadcrumbLoadPromise?: Promise<void>;
	private _rootEntityType?: string;

	/**
	 * Answers `UMB_ENTITY_CONTEXT` for the node being browsed. Without it the collection would bind to the entity of
	 * whichever element opened the modal, because a modal re-dispatches unanswered context requests onto its opener.
	 */
	#entityContext = new UmbEntityContext(this);

	#collectionConfiguration = new UmbContentCollectionConfigurationContext(this);

	#collectionMemories: Array<UmbInteractionMemoryModel> = [];

	protected _pickerContext = new UmbContentPickerContext(this);

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
		this.#observeCollectionInteractionMemories();
		this.#observeCollectionConfiguration();
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

			this.#collectionConfiguration.setCollectionAlias(this.data?.collection?.alias);

			if (this.data?.treeAlias && this.data.treeAlias !== this._treeAlias) {
				this._treeAlias = this.data.treeAlias;
				this._initialStartNode = this.data.startNode;
				this._currentLocation = this.data.startNode;
				this._breadcrumb = [];
				this._breadcrumbLoaded = false;
				this._breadcrumbLoadPromise = undefined;
				this.#initRepository(this.data.treeAlias);
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

	#initRepository(treeAlias: string) {
		const treeManifest = umbExtensionsRegistry.getByAlias<ManifestTree>(treeAlias);
		const repositoryAlias = treeManifest?.meta?.repositoryAlias;
		if (!repositoryAlias) return;

		new UmbExtensionApiInitializer<ManifestRepository<UmbTreeRepository>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this],
			async (permitted, ctrl) => {
				this._repository = permitted ? ctrl.api : undefined;
				if (this._repository && !this._breadcrumbLoaded) {
					this._breadcrumbLoaded = true;
					this._breadcrumbLoadPromise = this.#loadInitialBreadcrumb();
					await this._breadcrumbLoadPromise;
					await this.#restoreLocationFromMemory();
				}
			},
		);
	}

	async #loadInitialBreadcrumb() {
		if (!this._repository) return;

		if (this._initialStartNode) {
			const { data } = await this._repository.requestTreeItemAncestors({
				treeItem: this._initialStartNode,
			});
			const items = data ?? [];
			const ceilingIndex = items.findIndex((item) => item.unique === this._initialStartNode!.unique);
			const sliced = ceilingIndex >= 0 ? items.slice(ceilingIndex) : items;
			this._breadcrumb = sliced.map((item) => this.#toBreadcrumbItem(item));
			this._currentLocation = this.#toLocation(this._initialStartNode, items);
		} else {
			const { data: root } = await this._repository.requestTreeRoot();
			if (root) {
				this._rootEntityType = root.entityType;
				this._breadcrumb = [{ unique: null, entityType: root.entityType, name: root.name }];
				this.#resolveLocation();
			}
		}
	}

	#onItemOpen = async (event: UmbTreeItemOpenEvent) => {
		event.stopPropagation();
		const { unique, entityType } = event;
		await this.#navigateToLocation({ unique, entityType });
		this.#setLocationInInteractionMemory();
	};

	async #navigateToLocation(entity: UmbTreeStartNode) {
		if (!this._repository) {
			this._currentLocation = entity;
			return;
		}

		await this._breadcrumbLoadPromise;

		const { data } = await this._repository.requestTreeItemAncestors({ treeItem: entity });
		const items = data ?? [];

		if (this._initialStartNode) {
			const ceilingIndex = items.findIndex((item) => item.unique === this._initialStartNode!.unique);
			const sliced = ceilingIndex >= 0 ? items.slice(ceilingIndex) : items;
			this._breadcrumb = sliced.map((item) => this.#toBreadcrumbItem(item));
		} else {
			const root = this._breadcrumb[0];
			this._breadcrumb = [...(root ? [root] : []), ...items.map((item) => this.#toBreadcrumbItem(item))];
		}

		// Assigned once, and only once the ancestors are known, so the renderer never shows a node under the previous
		// node's configuration.
		this._currentLocation = this.#toLocation(entity, items);
	}

	/**
	 * The ancestors response ends with the requested item itself, which is where a node's own collection reference is
	 * read from.
	 * @param {UmbTreeStartNode} entity - The node being browsed.
	 * @param {Array<UmbTreeItemModel>} ancestors - The ancestors response for that node.
	 * @returns {UmbContentPickerLocation} The location to browse.
	 */
	#toLocation(entity: UmbTreeStartNode, ancestors: Array<UmbTreeItemModel>): UmbContentPickerLocation {
		const self = ancestors.find((item) => item.unique === entity.unique);
		return { ...entity, collectionUnique: this.#getCollectionUnique(self) };
	}

	#toBreadcrumbItem(item: UmbTreeItemModel): UmbContentPickerBreadcrumbItem {
		return {
			unique: item.unique,
			entityType: item.entityType,
			name: item.name,
			collectionUnique: this.#getCollectionUnique(item),
		};
	}

	#getCollectionUnique(item?: UmbTreeItemModel): string | undefined {
		return (item as (UmbTreeItemModel & UmbContentTreeItemLike) | undefined)?.contentType?.collection?.unique;
	}

	/**
	 * Points the entity context and the collection configuration at the node being browsed, and picks up that node's
	 * remembered collection state. The root renders the tree, as it has no content type and therefore no collection.
	 */
	#resolveLocation() {
		const location = this._currentLocation;

		this.#entityContext.setEntityType(location?.entityType ?? this._rootEntityType);
		this.#entityContext.setUnique(location?.unique ?? null);

		this.#collectionConfiguration.setUnique(location?.unique ?? null);
		this.#collectionConfiguration.setDataTypeUnique(location?.collectionUnique);

		this.#updateCollectionInteractionMemories();
	}

	#setLocationInInteractionMemory() {
		if (!this._currentLocation) {
			this._pickerContext.interactionMemory.deleteMemory(LOCATION_MEMORY_UNIQUE);
			return;
		}
		const memory: UmbInteractionMemoryModel = {
			unique: LOCATION_MEMORY_UNIQUE,
			value: {
				entity: {
					unique: this._currentLocation.unique,
					entityType: this._currentLocation.entityType,
				},
			},
		};
		this._pickerContext.interactionMemory.setMemory(memory);
	}

	#getLocationFromInteractionMemory(): UmbTreeStartNode | undefined {
		const memory = this._pickerContext.interactionMemory.getMemory(LOCATION_MEMORY_UNIQUE);
		return memory?.value?.entity;
	}

	async #restoreLocationFromMemory() {
		const entity = this.#getLocationFromInteractionMemory();
		if (!entity || !this._repository) return;

		if (this._initialStartNode) {
			const { data } = await this._repository.requestTreeItemAncestors({ treeItem: entity });
			const isWithinStartNode = (data ?? []).some((a) => a.unique === this._initialStartNode!.unique);
			if (!isWithinStartNode) return;
		}

		await this.#navigateToLocation(entity);
	}

	#onBreadcrumbItemClick(index: number) {
		if (index === this._breadcrumb.length - 1) return;

		const item = this._breadcrumb[index];
		if (index === 0 && !this._initialStartNode) {
			this._currentLocation = undefined;
		} else {
			this._currentLocation = {
				unique: item.unique!,
				entityType: item.entityType,
				collectionUnique: item.collectionUnique,
			};
		}
		this._breadcrumb = this._breadcrumb.slice(0, index + 1);
		this.#setLocationInInteractionMemory();
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

	override render() {
		return html`
			<umb-body-layout
				headline=${this.localize.string(this.data?.headline ?? '#general_choose')}
				?main-no-padding=${this._hasCollection}>
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
		if (!this._hasCollection) return this.#renderTree();
		// The configuration is resolved from a data type, so the collection is held back rather than briefly showing
		// the tree in its place.
		return this._collectionConfig ? this.#renderCollection() : html`<umb-view-loader></umb-view-loader>`;
	}

	#renderTree() {
		return html`
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
					startNode: this._currentLocation,
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
				alias=${ifDefined(this.data?.collection.alias)}
				.config=${config}
				.interactionMemories=${this._collectionInteractionMemories}
				@selected=${this.#onItemSelected}
				@deselected=${this.#onItemDeselected}
				@interaction-memories-change=${this.#onCollectionInteractionMemoriesChange}></umb-collection>`,
		);
	}

	#renderBreadcrumb() {
		if (!this._breadcrumb.length) return nothing;

		return html`
			<div id="breadcrumb">
				<uui-breadcrumbs>
					${repeat(
						this._breadcrumb,
						(item) => item.unique ?? ROOT_MEMORY_KEY,
						(item, index) => html`
							<uui-breadcrumb-item
								?last-item=${index === this._breadcrumb.length - 1}
								@click=${() => this.#onBreadcrumbItemClick(index)}>
								${this.localize.string(item.name)}
							</uui-breadcrumb-item>
						`,
					)}
				</uui-breadcrumbs>
			</div>
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

export default UmbContentPickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-picker-modal': UmbContentPickerModalElement<UmbTreeItemModelBase>;
	}
}
