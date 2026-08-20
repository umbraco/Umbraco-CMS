import type { UmbTreeItemPickerBreadcrumbItem } from '../components/tree-item-picker-breadcrumb/index.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { ManifestTree } from '../extensions/types.js';
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
	#startNode?: UmbTreeStartNode;
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
		this.#startNode = startNode;
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
					this.#loadPromise = this.#loadInitialTrail();
					await this.#loadPromise;
					await this.#restoreFromMemory();
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

	async #loadInitialTrail() {
		if (!this.#repository) return;

		if (this.#startNode) {
			const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: this.#startNode });
			const items = data ?? [];
			const node = items.find((item) => item.unique === this.#startNode!.unique);

			if (!node) {
				this.#currentLocation.setValue(null);
				return;
			}

			this.#setTrail(this.#toTrail(items));
			this.#currentLocation.setValue(node);
			return;
		}

		const { data: root } = await this.#repository.requestTreeRoot();
		if (!root) return;

		this.#setTrail([root]);
		this.#currentLocation.setValue(root);
	}

	/**
	 * Maps an ancestors response to a trail, cut off at the start node when there is one and rooted at the tree root
	 * when there is not.
	 * @param {Array<UmbTreeItemModel>} items - The ancestors response.
	 * @returns {Array<UmbTreeItemPickerLocation>} The trail.
	 */
	#toTrail(items: Array<UmbTreeItemModel>): Array<UmbTreeItemPickerLocation> {
		if (this.#startNode) {
			const ceilingIndex = items.findIndex((item) => item.unique === this.#startNode!.unique);
			return ceilingIndex >= 0 ? items.slice(ceilingIndex) : items;
		}

		const root = this.#trail.getValue().find((node) => node.unique === null);
		return root ? [root, ...items] : items;
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

	async #restoreFromMemory() {
		const entity: UmbTreeStartNode | undefined =
			this.#interactionMemoryManager?.getMemory(LOCATION_MEMORY_UNIQUE)?.value?.entity;
		if (!entity || !this.#repository) return;

		if (this.#startNode) {
			const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: entity });
			const isWithinStartNode = (data ?? []).some((ancestor) => ancestor.unique === this.#startNode!.unique);
			if (!isWithinStartNode) return;
		}

		await this.navigateTo(entity);

		// Opening a picker is not a request for that node, so a remembered location that has since been deleted is
		// quietly dropped rather than presented to the user as something they asked for.
		if (this.#currentLocation.getValue() === null) {
			this.#setLocationAndMemory(this.#resolveStart());
		}
	}
}
