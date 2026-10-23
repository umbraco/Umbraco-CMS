import type { UmbTreeItemPickerBreadcrumbItem } from '../components/tree-item-picker-breadcrumb/index.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { ManifestTree } from '../extensions/types.js';
import { resolveTreeStartNodes } from '../utils/index.js';
import type { UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from '../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry, type ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbInteractionMemoryManager,
	UmbInteractionMemoryModel,
} from '@umbraco-cms/backoffice/interaction-memory';

const LOCATION_MEMORY_UNIQUE = 'UmbTreeItemPickerLocation';

/**
 * The node a picker is browsing. It is the tree's own model of that node, so anything a consumer derives from it — the
 * collection a content type configures, for instance — describes the node being browsed and cannot describe another.
 *
 * The tree root is a location like any other, which is what lets an undefined location mean only that the picker has
 * not established one.
 */
export type UmbTreeItemPickerLocation = UmbTreeItemModel | UmbTreeRootModel;

/**
 * Construction arguments for {@link UmbTreeItemPickerLocationManager}.
 */
export interface UmbTreeItemPickerLocationManagerArgs {
	/**
	 * When provided, the location the user left the picker in is remembered and restored.
	 */
	interactionMemoryManager?: UmbInteractionMemoryManager;
}

/**
 * Tracks where in a tree a picker is browsing: the trail to it, the node itself, and the memory of both.
 */
export class UmbTreeItemPickerLocationManager extends UmbControllerBase {
	#trail = new UmbArrayState<UmbTreeItemPickerLocation>([], (x) => x.unique ?? 'root');

	#breadcrumb = new UmbArrayState<UmbTreeItemPickerBreadcrumbItem>([], (x) => x.unique ?? 'root');
	public readonly breadcrumb = this.#breadcrumb.asObservable();

	#currentLocation = new UmbObjectState<UmbTreeItemPickerLocation | null | undefined>(undefined);
	/**
	 * Where the picker is browsing, as a tri-state:
	 *
	 * - `undefined` — no location established yet.
	 * - `null` — the node last browsed to is not in the tree. A host should say so rather than show a level it cannot
	 *   describe, and must not treat this as still loading.
	 * - a model — the node being browsed.
	 */
	public readonly currentLocation = this.#currentLocation.asObservable();

	#interactionMemoryManager?: UmbInteractionMemoryManager;
	#treeAlias?: string;
	#configuredStartNode?: UmbTreeStartNode;
	#configuredStartNodes?: Array<UmbTreeStartNode>;
	#startNode?: UmbTreeStartNode;
	#startNodes?: Array<UmbTreeStartNode>;
	#repository?: UmbTreeRepository;
	#loaded = false;
	#loadPromise?: Promise<void>;

	/**
	 * @param {UmbControllerHost} host - The controller host this manager is bound to.
	 * @param {UmbTreeItemPickerLocationManagerArgs} [args] - Optional construction arguments.
	 */
	constructor(host: UmbControllerHost, args?: UmbTreeItemPickerLocationManagerArgs) {
		super(host);
		this.#interactionMemoryManager = args?.interactionMemoryManager;
	}

	/**
	 * Sets the node the picker may not browse above. Set it before the tree alias, as the alias starts the load.
	 * @param {UmbTreeStartNode} [startNode] - The node to start from, or undefined to browse from the tree root.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	setStartNode(startNode: UmbTreeStartNode | undefined) {
		this.#configuredStartNode = startNode;
		this.#resolveStartNodes();
	}

	/**
	 * Sets several nodes the picker may not browse above, browsed from the tree root. Set it before the tree alias, as
	 * the alias starts the load. Takes precedence over the start node, and a single node is handled as the start node.
	 * @param {Array<UmbTreeStartNode>} [startNodes] - The nodes to start from.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	setStartNodes(startNodes: Array<UmbTreeStartNode> | undefined) {
		this.#configuredStartNodes = startNodes;
		this.#resolveStartNodes();
	}

	/**
	 * Returns the single node the picker is scoped to.
	 * @returns {UmbTreeStartNode | undefined} The start node, or undefined when the picker is scoped to several nodes or
	 * to none.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	getStartNode(): UmbTreeStartNode | undefined {
		return this.#startNode;
	}

	/**
	 * Returns the nodes the picker is scoped to when it is scoped to more than one.
	 * @returns {Array<UmbTreeStartNode> | undefined} Two or more start nodes, or undefined otherwise.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	getStartNodes(): Array<UmbTreeStartNode> | undefined {
		return this.#startNodes;
	}

	/**
	 * Determines whether a node is only there to be browsed into. When the picker is scoped to several start nodes they
	 * take the place of the tree root, so they cannot be picked themselves.
	 * @param {string | null} [unique] - The unique of the node.
	 * @returns {boolean} True when the node is one of several start nodes.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	isBrowseOnlyStartNode(unique: string | null | undefined): boolean {
		if (!unique || !this.#startNodes) return false;
		return this.#startNodes.some((startNode) => startNode.unique === unique);
	}

	/**
	 * Determines whether an item that was not reached by browsing the tree, such as a search result, lies within the
	 * start nodes. With several start nodes the item must have one of them among its ancestors, so an item that carries
	 * no ancestors is out of scope. With a single start node or none, the scope is applied where the items are fetched,
	 * so every item is within it.
	 * @param {{ unique?: string | null; ancestors?: Array<{ unique: string | null }> }} item - The item to check.
	 * @param {string | null} [item.unique] - The unique of the item.
	 * @param {Array<{ unique: string | null }>} [item.ancestors] - The ancestors of the item.
	 * @returns {boolean} True when the item lies within the start nodes.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	isWithinStartNodes(item: { unique?: string | null; ancestors?: Array<{ unique: string | null }> }): boolean {
		if (!this.#startNodes) return true;
		if (this.isBrowseOnlyStartNode(item.unique)) return false;
		return item.ancestors?.some((ancestor) => this.isBrowseOnlyStartNode(ancestor.unique)) ?? false;
	}

	#resolveStartNodes() {
		const { startNode, startNodes } = resolveTreeStartNodes(this.#configuredStartNode, this.#configuredStartNodes);
		this.#startNode = startNode;
		this.#startNodes = startNodes;
	}

	/**
	 * Sets the tree to browse, which resolves its repository and loads the initial trail.
	 * @param {string} [treeAlias] - The alias of the tree.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	setTreeAlias(treeAlias: string | undefined) {
		if (!treeAlias || treeAlias === this.#treeAlias) return;

		this.#treeAlias = treeAlias;
		this.#setTrail([]);
		this.#currentLocation.setValue(undefined);
		this.#loaded = false;
		this.#loadPromise = undefined;

		const repositoryAlias = umbExtensionsRegistry.getByAlias<ManifestTree>(treeAlias)?.meta?.repositoryAlias;
		// Without a repository there is no tree, so there is no location to establish either.
		if (!repositoryAlias) return;

		new UmbExtensionApiInitializer<ManifestRepository<UmbTreeRepository>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this],
			async (permitted, ctrl) => {
				this.#repository = permitted ? ctrl.api : undefined;
				if (this.#repository && !this.#loaded) {
					this.#loaded = true;

					// A remembered location is where the picker reopens, so the start is not committed when one is going to
					// replace it. Committing both would hand the host two locations in a row, and a host that renders a
					// different view per location would mount one for the start and tear it down again mid-initialisation.
					const remembered = this.#getRememberedEntity();

					this.#loadPromise = this.#loadInitialTrail(!remembered);
					await this.#loadPromise;

					// An empty trail means the start itself could not be established, which `#loadInitialTrail` has already
					// reported. There is nowhere to restore into.
					if (remembered && this.#trail.getValue().length) {
						await this.#restoreFromMemory(remembered);
						// Committed here rather than by the initial trail, so a restore that did not land still leaves the
						// picker somewhere.
						if (this.#currentLocation.getValue() === undefined) {
							this.#setLocationAndMemory(this.#resolveStart());
						}
					}
				}
			},
		);
	}

	/**
	 * Returns the node currently being browsed.
	 * @returns {UmbTreeItemPickerLocation | null | undefined} The current location, undefined before one is established,
	 * or null when the node last browsed to is not in the tree.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	getCurrentLocation(): UmbTreeItemPickerLocation | null | undefined {
		return this.#currentLocation.getValue();
	}

	/**
	 * Returns the trail to the node currently being browsed.
	 * @returns {Array<UmbTreeItemPickerBreadcrumbItem>} The trail, ending with the node being browsed.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	getBreadcrumb(): Array<UmbTreeItemPickerBreadcrumbItem> {
		return this.#breadcrumb.getValue();
	}

	/**
	 * Browses to a node, leaving the trail, the current location and the remembered location consistent with each other.
	 *
	 * A node already in the trail is browsed by shortening the trail to it, so stepping back up costs no request. A node
	 * the tree does not have leaves the current location `null`.
	 * @param {UmbEntityModel} [entity] - The node to browse, or undefined for the start node when there is one and the
	 * tree root otherwise.
	 * @returns {Promise<void>} Resolves once the trail and the current location have been set.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	async navigateTo(entity?: UmbEntityModel): Promise<void> {
		// The initial trail establishes the root and the ceiling, so it has to be in place first.
		await this.#loadPromise;

		if (!entity?.unique) {
			this.#setLocationAndMemory(this.#resolveStart());
			return;
		}

		const node = await this.#resolve({ unique: entity.unique, entityType: entity.entityType });

		if (!node) {
			// Somewhere we cannot describe is worse than nowhere, so the location says something is wrong rather than
			// leaving a host to render a level it knows nothing about.
			this.#currentLocation.setValue(null);
			this.#deleteLocationMemory();
			return;
		}

		this.#setLocationAndMemory(node);
	}

	/**
	 * Browsing to no node means returning to wherever the picker starts, which is the first step of the trail.
	 * @returns {UmbTreeItemPickerLocation | undefined} The start node or the tree root, whichever the picker starts at.
	 */
	#resolveStart(): UmbTreeItemPickerLocation | undefined {
		const trail = this.#trail.getValue();
		if (!trail.length) return undefined;

		this.#setTrail(trail.slice(0, 1));
		return trail[0];
	}

	/**
	 * Finds the node in the trail, shortening it, or rebuilds the trail from the tree.
	 * @param {UmbTreeStartNode} entity - The node to browse.
	 * @returns {Promise<UmbTreeItemPickerLocation | undefined>} The node, or undefined when the tree does not have it.
	 */
	async #resolve(entity: UmbTreeStartNode): Promise<UmbTreeItemPickerLocation | undefined> {
		const trail = this.#trail.getValue();
		const index = trail.findIndex((node) => node.unique === entity.unique);

		if (index >= 0) {
			this.#setTrail(trail.slice(0, index + 1));
			return trail[index];
		}

		if (!this.#repository) return undefined;

		const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: entity });
		const items = data ?? [];
		const node = items.find((item) => item.unique === entity.unique);

		// The trail is left as it was when the node is not there, so a host can still show where the user came from.
		if (!node) return undefined;

		this.#setTrail(this.#toTrail(items));
		return node;
	}

	/**
	 * Establishes the trail the picker starts from, and the location with it unless a restore is going to supersede it.
	 * @param {boolean} commitLocation - Whether to report the start as the current location.
	 */
	async #loadInitialTrail(commitLocation: boolean) {
		if (!this.#repository) return;

		if (this.#startNode) {
			const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: this.#startNode });
			const items = data ?? [];
			const node = items.find((item) => item.unique === this.#startNode!.unique);

			// A start the tree does not have is reported whether or not a restore follows: there is no trail to restore
			// into, so the failure is the outcome.
			if (!node) {
				this.#currentLocation.setValue(null);
				return;
			}

			this.#setTrail(this.#toTrail(items));
			if (commitLocation) this.#currentLocation.setValue(node);
			return;
		}

		const { data: root } = await this.#repository.requestTreeRoot();
		if (!root) return;

		this.#setTrail([root]);
		if (commitLocation) this.#currentLocation.setValue(root);
	}

	/**
	 * Maps an ancestors response to a trail, cut off at the start node when there is one and rooted at the tree root
	 * when there is not.
	 * @param {Array<UmbTreeItemModel>} items - The ancestors response.
	 * @returns {Array<UmbTreeItemPickerLocation>} The trail.
	 */
	#toTrail(items: Array<UmbTreeItemModel>): Array<UmbTreeItemPickerLocation> {
		const sliced = this.#sliceToScope(items);
		if (this.#startNode) return sliced;

		const root = this.#trail.getValue().find((node) => node.unique === null);
		return root ? [root, ...sliced] : sliced;
	}

	// Ancestors above the scope of the picker must not be reachable from the trail, so the chain is cut at the start
	// node it belongs to — with multiple start nodes, whichever of them is an ancestor or self.
	#sliceToScope(items: Array<UmbTreeItemModel>): Array<UmbTreeItemModel> {
		const scopeUniques = this.#getScopeUniques();
		if (scopeUniques.length === 0) return items;

		const ceilingIndex = items.findIndex((item) => item.unique !== null && scopeUniques.includes(item.unique));
		return ceilingIndex >= 0 ? items.slice(ceilingIndex) : items;
	}

	#getScopeUniques(): Array<string> {
		if (this.#startNode) return [this.#startNode.unique];
		return this.#startNodes?.map((startNode) => startNode.unique) ?? [];
	}

	#setTrail(trail: Array<UmbTreeItemPickerLocation>) {
		this.#trail.setValue(trail);
		this.#breadcrumb.setValue(
			trail.map((node) => ({ unique: node.unique, entityType: node.entityType, name: node.name })),
		);
	}

	/**
	 * Only `navigateTo` does this: loading the initial trail must not overwrite the location a previous session left
	 * behind, as that is what gets restored.
	 * @param {UmbTreeItemPickerLocation} [location] - The location the user browsed to.
	 */
	#setLocationAndMemory(location: UmbTreeItemPickerLocation | undefined) {
		this.#currentLocation.setValue(location);

		// The root is where a picker starts, so it is not worth a memory.
		if (!location?.unique) {
			this.#deleteLocationMemory();
			return;
		}

		const memory: UmbInteractionMemoryModel = {
			unique: LOCATION_MEMORY_UNIQUE,
			value: { entity: { unique: location.unique, entityType: location.entityType } },
		};
		this.#interactionMemoryManager?.setMemory(memory);
	}

	#deleteLocationMemory() {
		this.#interactionMemoryManager?.deleteMemory(LOCATION_MEMORY_UNIQUE);
	}

	#getRememberedEntity(): UmbTreeStartNode | undefined {
		return this.#interactionMemoryManager?.getMemory(LOCATION_MEMORY_UNIQUE)?.value?.entity;
	}

	/**
	 * Browses to the location a previous session left behind.
	 * @param {UmbTreeStartNode} entity - The remembered node.
	 */
	async #restoreFromMemory(entity: UmbTreeStartNode) {
		if (!this.#repository) return;

		const scopeUniques = this.#getScopeUniques();
		if (scopeUniques.length > 0) {
			const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: entity });
			const isWithinScope = (data ?? []).some((ancestor) => scopeUniques.includes(ancestor.unique));
			if (!isWithinScope) return;
		}

		await this.navigateTo(entity);

		// Opening a picker is not a request for that node, so a remembered location that has since been deleted is
		// quietly dropped rather than presented to the user as something they asked for.
		if (this.#currentLocation.getValue() === null) {
			this.#setLocationAndMemory(this.#resolveStart());
		}
	}
}
