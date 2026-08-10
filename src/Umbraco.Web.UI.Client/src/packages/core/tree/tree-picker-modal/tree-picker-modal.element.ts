import { UmbTreeItemPickerContext } from '../tree-item-picker/index.js';
import type { UmbTreeElement } from '../tree.element.js';
import type { UmbTreeItemModelBase, UmbTreeSelectionConfiguration, UmbTreeStartNode } from '../types.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { ManifestTree } from '../extensions/types.js';
import { UmbTreeItemOpenEvent } from '../tree-item/events/tree-item-open.event.js';
import { umbResolveTreeStartNodes } from '../utils/index.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './types.js';
import { css, customElement, html, ifDefined, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbPickerModalBaseElement } from '@umbraco-cms/backoffice/picker';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityExpansionModel, UmbExpansionChangeEvent } from '@umbraco-cms/backoffice/utils';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

const TREE_MEMORY_UNIQUE = 'UmbTreeItemPickerTree';
const LOCATION_MEMORY_UNIQUE = 'UmbTreeItemPickerLocation';

interface UmbTreeBreadcrumbItem {
	unique: string | null;
	entityType: string;
	name: string;
}

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
	private _treeInteractionMemories?: Array<UmbInteractionMemoryModel>;

	@state()
	private _currentLocation?: UmbTreeStartNode;

	@state()
	private _breadcrumb: Array<UmbTreeBreadcrumbItem> = [];

	#treeAlias?: string;
	private _initialStartNode?: UmbTreeStartNode;
	private _initialStartNodes?: Array<UmbTreeStartNode>;
	private _repository?: UmbTreeRepository;
	private _breadcrumbLoaded = false;
	private _breadcrumbLoadPromise?: Promise<void>;

	protected _pickerContext = new UmbTreeItemPickerContext(this);

	constructor() {
		super();
		this._pickerContext.selection.setSelectable(true);
		this.observe(this._pickerContext.selection.hasSelection, (hasSelection) => {
			this._hasSelection = hasSelection;
		});
		this.#observePickerSelection();
		this.#observeSearch();
		this.#observeExpansion();
		this.#observeTreeInteractionMemories();
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
			const resolvedStartNodes = umbResolveTreeStartNodes(this.data?.startNode, this.data?.startNodes);
			this.#startNodeUniques = new Set(resolvedStartNodes.startNodes?.map((startNode) => startNode.unique) ?? []);

			if (this.data?.search) {
				this._pickerContext.search.updateConfig({
					...this.data.search,
					// There is no server side search across multiple parents, so the scope is applied client side.
					searchFrom: resolvedStartNodes.startNode,
					dataTypeUnique: this._pickerContext.dataType?.unique,
				});
			}

			const multiple = this.data?.multiple ?? false;
			this._pickerContext.selection.setMultiple(multiple);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				multiple,
			};

			if (this.data?.treeAlias && this.data.treeAlias !== this.#treeAlias) {
				this.#treeAlias = this.data.treeAlias;
				const { startNode, startNodes } = umbResolveTreeStartNodes(this.data.startNode, this.data.startNodes);
				this._initialStartNode = startNode;
				this._initialStartNodes = startNodes;
				this._currentLocation = startNode;
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

	// Ancestors above the scope of the picker must not be reachable from the breadcrumb, so the chain is cut at
	// the start node it belongs to — with multiple start nodes, whichever of them is an ancestor or self.
	#sliceAncestorsToScope<ItemType extends { unique: string | null }>(items: Array<ItemType>): Array<ItemType> {
		const scopeUniques = this._initialStartNode
			? [this._initialStartNode.unique]
			: (this._initialStartNodes?.map((startNode) => startNode.unique) ?? []);

		if (scopeUniques.length === 0) return items;

		const ceilingIndex = items.findIndex((item) => item.unique !== null && scopeUniques.includes(item.unique));
		return ceilingIndex >= 0 ? items.slice(ceilingIndex) : items;
	}

	async #loadInitialBreadcrumb() {
		if (!this._repository) return;

		if (this._initialStartNode) {
			const { data } = await this._repository.requestTreeItemAncestors({
				treeItem: this._initialStartNode,
			});
			this._breadcrumb = this.#sliceAncestorsToScope(data ?? []).map((item) => ({
				unique: item.unique,
				entityType: item.entityType,
				name: item.name,
			}));
		} else {
			const { data: root } = await this._repository.requestTreeRoot();
			if (root) {
				this._breadcrumb = [{ unique: null, entityType: root.entityType, name: root.name }];
			}
		}
	}

	#onTreeItemOpen = async (event: UmbTreeItemOpenEvent) => {
		event.stopPropagation();
		const { unique, entityType } = event;
		await this.#navigateToLocation({ unique, entityType });
		this.#setLocationInInteractionMemory();
	};

	async #navigateToLocation(entity: UmbTreeStartNode) {
		this._currentLocation = entity;
		if (!this._repository) return;

		await this._breadcrumbLoadPromise;

		const { data } = await this._repository.requestTreeItemAncestors({ treeItem: entity });
		const items = data ?? [];

		if (this._initialStartNode) {
			this._breadcrumb = this.#sliceAncestorsToScope(items).map((item) => ({
				unique: item.unique,
				entityType: item.entityType,
				name: item.name,
			}));
		} else {
			const root = this._breadcrumb[0];
			this._breadcrumb = [
				...(root ? [root] : []),
				...this.#sliceAncestorsToScope(items).map((item) => ({
					unique: item.unique,
					entityType: item.entityType,
					name: item.name,
				})),
			];
		}
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

		const scopeUniques = this._initialStartNode
			? [this._initialStartNode.unique]
			: (this._initialStartNodes?.map((startNode) => startNode.unique) ?? []);

		if (scopeUniques.length > 0) {
			const { data } = await this._repository.requestTreeItemAncestors({ treeItem: entity });
			const isWithinScope = (data ?? []).some((ancestor) => scopeUniques.includes(ancestor.unique));
			if (!isWithinScope) return;
		}

		await this.#navigateToLocation(entity);
	}

	#onBreadcrumbItemClick(index: number) {
		if (index === this._breadcrumb.length - 1) return;

		const item = this._breadcrumb[index];
		if (index === 0 && !this._initialStartNode) {
			this._currentLocation = undefined;
		} else {
			this._currentLocation = { unique: item.unique!, entityType: item.entityType };
		}
		this._breadcrumb = this._breadcrumb.slice(0, index + 1);
		this.#setLocationInInteractionMemory();
	}

	#observePickerSelection() {
		this.observe(
			this._pickerContext.selection.selection,
			(selection) => {
				this.updateValue({ selection });
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

	#startNodeUniques = new Set<string>();

	// The start nodes are only there to be browsed, so they are not pickable — matching the single start node
	// case, where the start node is not rendered at all.
	#selectableFilter = (item: TreeItemType) => {
		const unique = (item as { unique?: string | null }).unique;
		if (unique && this.#startNodeUniques.has(unique)) return false;
		return this.data?.pickableFilter?.(item) ?? true;
	};

	// Search cannot be scoped server side to more than one parent, so out-of-scope results are filtered here.
	#searchSelectableFilter = (item: UmbSearchResultItemModel & TreeItemType) => {
		const baseFilter: ((item: UmbSearchResultItemModel & TreeItemType) => boolean) | undefined =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter;
		if (baseFilter && !baseFilter(item)) return false;
		if (this.#startNodeUniques.size === 0) return true;

		const unique = item.unique;
		if (unique && this.#startNodeUniques.has(unique)) return false;

		// Without a resolved ancestor chain there is nothing to compare the scope against.
		const ancestors = (item as { ancestors?: Array<{ unique: string }> }).ancestors;
		if (!ancestors) return true;

		return ancestors.some((ancestor) => this.#startNodeUniques.has(ancestor.unique));
	};

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.string(this.data?.headline ?? '#general_choose')}>
				${this.#renderSearch()} ${this.#renderTree()} ${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderSearch() {
		return html`
			<umb-picker-search-field></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${this.#searchSelectableFilter}></umb-picker-search-result>
		`;
	}

	#renderTree() {
		if (this._searchQuery) {
			return nothing;
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
					selectableFilter: this.#selectableFilter,
					startNode: this._currentLocation,
					// Once drilled into a location the tree is scoped to that single node again.
					startNodes: this._currentLocation ? undefined : this._initialStartNodes,
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
		if (!this._breadcrumb.length) return nothing;

		return html`
			<div id="breadcrumb">
				<uui-breadcrumbs>
					${repeat(
						this._breadcrumb,
						(item) => item.unique ?? 'root',
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
		#breadcrumb {
			margin-bottom: var(--uui-size-space-4);
		}

		uui-breadcrumbs {
			overflow: hidden;
			min-width: 0;
		}

		uui-breadcrumb-item:not([last-item]) {
			cursor: pointer;
		}
	`;
}

export default UmbTreePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-picker-modal': UmbTreePickerModalElement<UmbTreeItemModelBase>;
	}
}
