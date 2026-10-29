import { UMB_DOCUMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/constants.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UMB_CONTENT_SECTION_ALIAS, UmbContentCollectionConfigurationContext } from '@umbraco-cms/backoffice/content';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { css, customElement, html, keyed, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_TREE_ITEM_PICKER_CONTEXT, UmbTreeItemOpenEvent } from '@umbraco-cms/backoffice/tree';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
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
 * The part of a content tree item this element reads to decide how a node's children render. A tree that does not map
 * `contentType` is browsed as a tree throughout.
 */
type UmbContentTreeItemLike = {
	contentType?: {
		collection?: { unique: string } | null;
	};
};

/**
 * Browses documents for a picker: a breadcrumb of the location being browsed, and below it the document collection at
 * any level whose node has one configured (and the user has Content-section access), or the tree everywhere else.
 *
 * Must be placed inside a tree item picker context (`UMB_TREE_ITEM_PICKER_CONTEXT`), which holds the location,
 * selection, expansion and interaction memory this element reads and writes. The host owns that context and configures
 * it — the document tree, start nodes, whether multiple items can be picked — before or after this element connects.
 * @element umb-document-picker-browse
 * @fires {UmbSelectedEvent} selected - When an item is picked.
 * @fires {UmbDeselectedEvent} deselected - When an item is unpicked.
 */
@customElement('umb-document-picker-browse')
export class UmbDocumentPickerBrowseElement extends UmbLitElement {
	/**
	 * Determines whether an item can be picked. Items it rejects stay visible but cannot be picked.
	 */
	@property({ attribute: false })
	pickableFilter?: (item: UmbDocumentTreeItemModel) => boolean;

	/**
	 * Hides the tree root, so the tree starts with the root's children.
	 */
	@property({ type: Boolean, attribute: 'hide-tree-root' })
	hideTreeRoot?: boolean;

	/**
	 * Expands the tree root when the tree first renders.
	 */
	@property({ type: Boolean, attribute: 'expand-tree-root' })
	expandTreeRoot?: boolean;

	/**
	 * Passed on to the tree as is. The built-in tree does not apply it.
	 * @deprecated Use `pickableFilter` instead. Scheduled for removal in Umbraco 21.
	 */
	@property({ attribute: false })
	filter?: (item: UmbDocumentTreeItemModel) => boolean;

	/**
	 * The tree to browse in place of the document tree.
	 * @deprecated This element always browses the document tree. Scheduled for removal in Umbraco 21.
	 */
	@property({ type: String, attribute: 'tree-alias' })
	treeAlias?: string;

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

	/**
	 * Answers `UMB_ENTITY_CONTEXT` for the node being browsed. Without it the collection would bind to the entity of
	 * whatever this element sits in — in a modal, the element that opened it.
	 */
	#entityContext = new UmbEntityContext(this);

	#collectionConfiguration = new UmbContentCollectionConfigurationContext(this);

	#collectionMemories: Array<UmbInteractionMemoryModel> = [];

	#pickerContext?: typeof UMB_TREE_ITEM_PICKER_CONTEXT.TYPE;

	#onItemOpen = async (event: UmbTreeItemOpenEvent) => {
		event.stopPropagation();
		const { unique, entityType } = event;
		await this.#pickerContext?.location.navigateTo({ unique, entityType });
	};

	constructor() {
		super();
		// Which collection this element renders follows from it browsing documents, so it is not a host's choice.
		this.#collectionConfiguration.setCollectionAlias(UMB_DOCUMENT_COLLECTION_ALIAS);
		this.#observeCollectionConfiguration();
		this.#observeContentSectionAccess();

		this.consumeContext(UMB_TREE_ITEM_PICKER_CONTEXT, (context) => {
			this.#pickerContext = context;
			this.#observePickerContext();
		});
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

		// eslint-disable-next-line @typescript-eslint/no-deprecated
		if (_changedProperties.has('filter') && this.filter) {
			new UmbDeprecation({
				deprecated:
					'The `filter` property of umb-document-picker-browse (and the `filter` option of UmbDocumentPickerModalData) is not applied by the built-in tree.',
				removeInVersion: '21.0.0',
				solution: 'Use `pickableFilter` to prevent items from being picked instead.',
			}).warn();
		}

		// eslint-disable-next-line @typescript-eslint/no-deprecated
		if (_changedProperties.has('treeAlias') && this.treeAlias && this.treeAlias !== UMB_DOCUMENT_TREE_ALIAS) {
			new UmbDeprecation({
				deprecated:
					'The `treeAlias` property of umb-document-picker-browse (and the `treeAlias` option of UmbDocumentPickerModalData) is deprecated.',
				removeInVersion: '21.0.0',
				solution: 'The document picker always browses the document tree. Use a tree picker to browse another tree.',
			}).warn();
		}

		if (_changedProperties.has('pickableFilter')) {
			this._collectionSelectionConfiguration = {
				...this._collectionSelectionConfiguration,
				selectableFilter: this.pickableFilter as ((item: UmbCollectionItemModel) => boolean) | undefined,
			};
		}

		// The single point where a location is turned into a rendering decision. Everything that browses — a tree item
		// opening, a collection item drilling in, a breadcrumb click, a restore from memory — moves `_currentLocation`
		// and nothing else, so the renderer, the entity context and the configuration cannot drift apart.
		if (_changedProperties.has('_currentLocation')) {
			this.#resolveLocation();
		}
	}

	#observePickerContext() {
		const context = this.#pickerContext;

		this.observe(
			context?.location.currentLocation,
			(location) => {
				this._currentLocation = location;
				this._treeStartNode = location?.unique
					? { unique: location.unique, entityType: location.entityType }
					: undefined;
			},
			'umbDocumentPickerLocationObserver',
		);

		this.observe(
			context?.selection.multiple,
			(multiple) => {
				this._selectionConfiguration = { ...this._selectionConfiguration, multiple: multiple ?? false };
				this._collectionSelectionConfiguration = {
					...this._collectionSelectionConfiguration,
					multiple: multiple ?? false,
				};
			},
			'umbDocumentPickerMultipleObserver',
		);

		this.observe(
			context?.selection.selection,
			(selection) => {
				this._selectionConfiguration = { ...this._selectionConfiguration, selection: [...(selection ?? [])] };
				this._collectionSelectionConfiguration = {
					...this._collectionSelectionConfiguration,
					selection: (selection ?? []).filter((unique) => unique !== null),
				};
			},
			'umbDocumentPickerSelectionObserver',
		);

		this.observe(
			context?.expansion.expansion,
			(expansion) => {
				this._treeExpansion = expansion ?? [];
			},
			'umbDocumentPickerExpansionObserver',
		);

		this.observe(
			context?.interactionMemory.memory(TREE_MEMORY_UNIQUE),
			(memory) => {
				this._treeInteractionMemories = memory?.memories;
			},
			'umbDocumentPickerTreeInteractionMemoriesObserver',
		);

		this.observe(
			context?.interactionMemory.memory(COLLECTION_MEMORY_UNIQUE),
			(memory) => {
				this.#collectionMemories = memory?.memories ?? [];
				this.#updateCollectionInteractionMemories();
			},
			'umbDocumentPickerCollectionInteractionMemoriesObserver',
		);
	}

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
			this.#pickerContext?.interactionMemory.setMemory({ unique: COLLECTION_MEMORY_UNIQUE, memories: next });
		} else {
			this.#pickerContext?.interactionMemory.deleteMemory(COLLECTION_MEMORY_UNIQUE);
		}
	}

	#onItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#pickerContext?.selection.select(event.unique);
		this.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#pickerContext?.selection.deselect(event.unique);
		this.dispatchEvent(new UmbDeselectedEvent(event.unique));
	}

	#onTreeItemExpansionChange(event: UmbExpansionChangeEvent) {
		const target = event.target as UmbTreeElement;
		this.#pickerContext?.expansion.setExpansion(target.getExpansion());
	}

	#onTreeInteractionMemoriesChange(event: Event) {
		event.stopPropagation();
		const tree = event.currentTarget as UmbTreeElement;
		const memories = tree.interactionMemories;
		if (memories.length > 0) {
			this.#pickerContext?.interactionMemory.setMemory({ unique: TREE_MEMORY_UNIQUE, memories });
		} else {
			this.#pickerContext?.interactionMemory.deleteMemory(TREE_MEMORY_UNIQUE);
		}
	}

	#selectableFilter = (item: UmbDocumentTreeItemModel) => {
		if (this.#pickerContext?.location.isBrowseOnlyStartNode(item.unique)) return false;
		return this.pickableFilter?.(item) ?? true;
	};

	/** Whether the collection is actually being rendered for the browsed node, rather than merely configured for it. */
	#willRenderCollection(): boolean {
		return this._hasCollection && this._hasContentSectionAccess;
	}

	override render() {
		return html`<umb-tree-item-picker-breadcrumb id="breadcrumb"></umb-tree-item-picker-breadcrumb>
			${this.#renderChildren()}`;
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
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		const treeAlias = this.treeAlias ?? UMB_DOCUMENT_TREE_ALIAS;

		return html`
			<umb-tree
				alias=${treeAlias}
				.props=${{
					showToolbar: true,
					drillable: true,
					hideTreeItemActions: true,
					hideTreeRoot: this.hideTreeRoot,
					expandTreeRoot: this.expandTreeRoot,
					selectionConfiguration: this._selectionConfiguration,
					// eslint-disable-next-line @typescript-eslint/no-deprecated
					filter: this.filter,
					selectableFilter: this.#selectableFilter,
					startNode: this._treeStartNode,
					// Once drilled into a location the tree is scoped to that single node again.
					startNodes: this._treeStartNode ? undefined : this.#pickerContext?.location.getStartNodes(),
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

	static override styles = css`
		:host {
			display: block;
		}

		#breadcrumb {
			margin-bottom: var(--uui-size-space-4);
		}

		#not-found {
			text-align: center;
			padding: var(--uui-size-layout-1);
		}

		umb-collection {
			display: block;
			height: fit-content;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-picker-browse': UmbDocumentPickerBrowseElement;
	}
}
