import type { UmbTreeItemPickerBreadcrumbItem } from '../components/tree-item-picker-breadcrumb/index.js';
import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { ManifestTree } from '../extensions/types.js';
import type { UmbTreeItemModel, UmbTreeStartNode } from '../types.js';
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
 * The node a picker is browsing. A `unique` of `null` is the tree root, which is a place like any other rather than the
 * absence of one — so an undefined location means only that the picker has not established one yet.
 *
 * `treeItem` is the same node in full, and travels with it so a consumer deriving anything from it — the collection a
 * content type configures, for instance — cannot end up describing a different node than the one being browsed. It is
 * absent at the root and wherever the tree could not supply one.
 */
export type UmbTreeItemPickerLocation = UmbEntityModel & {
	treeItem?: UmbTreeItemModel;
};

/**
 * Construction arguments for {@link UmbTreeItemPickerLocationManager}.
 */
export interface UmbTreeItemPickerLocationManagerArgs {
	/**
	 * When provided, the location the user left the picker in is remembered and restored.
	 */
	interactionMemoryManager?: UmbInteractionMemoryManager;
}

/** One step of the trail, keeping the presented crumb and the item it came from index-aligned. */
type UmbTreeItemPickerTrailStep = {
	crumb: UmbTreeItemPickerBreadcrumbItem;
	item?: UmbTreeItemModel;
};

/**
 * Tracks where in a tree a picker is browsing: the trail to it, the node itself, and the memory of both.
 */
export class UmbTreeItemPickerLocationManager extends UmbControllerBase {
	#trail = new UmbArrayState<UmbTreeItemPickerTrailStep>([], (x) => x.crumb.unique ?? 'root');

	#breadcrumb = new UmbArrayState<UmbTreeItemPickerBreadcrumbItem>([], (x) => x.unique ?? 'root');
	public readonly breadcrumb = this.#breadcrumb.asObservable();

	#currentLocation = new UmbObjectState<UmbTreeItemPickerLocation | undefined>(undefined);
	public readonly currentLocation = this.#currentLocation.asObservable();

	#interactionMemoryManager?: UmbInteractionMemoryManager;
	#treeAlias?: string;
	#startNode?: UmbTreeStartNode;
	#repository?: UmbTreeRepository;
	#loaded = false;
	#loadPromise?: Promise<void>;
	#rootEntityType?: string;

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
		this.#trail.setValue([]);
		this.#breadcrumb.setValue([]);
		this.#currentLocation.setValue(undefined);
		this.#loaded = false;
		this.#loadPromise = undefined;
		this.#rootEntityType = undefined;

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
	 * @returns {UmbTreeItemPickerLocation | undefined} The current location, or undefined before one is established.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	getCurrentLocation(): UmbTreeItemPickerLocation | undefined {
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
	 * A node already in the trail is browsed by shortening the trail to it, so stepping back up costs no request and
	 * reuses the tree item that trail step already holds.
	 * @param {UmbEntityModel} [entity] - The node to browse, or undefined for the start node when there is one and the
	 * tree root otherwise.
	 * @returns {Promise<void>} Resolves once the trail and the current location have been set.
	 * @memberof UmbTreeItemPickerLocationManager
	 */
	async navigateTo(entity?: UmbEntityModel): Promise<void> {
		// The initial trail establishes the root and the ceiling, so it has to be in place first.
		await this.#loadPromise;

		const location = entity?.unique
			? await this.#resolve({ unique: entity.unique, entityType: entity.entityType })
			: this.#resolveStart();

		this.#setCurrentLocation(location);
		this.#remember(location);
	}

	/**
	 * Browsing to no node means returning to wherever the picker starts, which is the first step of the trail.
	 * @returns {UmbTreeItemPickerLocation | undefined} The start node or the tree root, whichever the picker starts at.
	 */
	#resolveStart(): UmbTreeItemPickerLocation | undefined {
		const trail = this.#trail.getValue();
		if (trail.length) {
			this.#setTrail(trail.slice(0, 1));
		}

		if (this.#startNode) return { ...this.#startNode, treeItem: trail[0]?.item };
		return this.#rootEntityType ? { unique: null, entityType: this.#rootEntityType } : undefined;
	}

	/**
	 * Finds the node in the trail, shortening it, or rebuilds the trail from the tree.
	 * @param {UmbTreeStartNode} entity - The node to browse.
	 * @returns {Promise<UmbTreeItemPickerLocation>} The location to move to.
	 */
	async #resolve(entity: UmbTreeStartNode): Promise<UmbTreeItemPickerLocation> {
		const trail = this.#trail.getValue();
		const index = trail.findIndex((step) => step.crumb.unique === entity.unique);

		if (index >= 0) {
			this.#setTrail(trail.slice(0, index + 1));
			return { ...entity, treeItem: trail[index].item };
		}

		if (!this.#repository) return { ...entity };

		const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: entity });
		const items = data ?? [];
		this.#setTrail(this.#toSteps(items));

		// Committed only once the ancestors are known, so a consumer never sees a node described by the previous
		// node's trail.
		return { ...entity, treeItem: items.find((item) => item.unique === entity.unique) };
	}

	async #loadInitialTrail() {
		if (!this.#repository) return;

		if (this.#startNode) {
			const { data } = await this.#repository.requestTreeItemAncestors({ treeItem: this.#startNode });
			const items = data ?? [];
			this.#setTrail(this.#toSteps(items));
			this.#setCurrentLocation({
				...this.#startNode,
				treeItem: items.find((item) => item.unique === this.#startNode!.unique),
			});
			return;
		}

		const { data: root } = await this.#repository.requestTreeRoot();
		if (!root) return;

		this.#rootEntityType = root.entityType;
		this.#setTrail([{ crumb: { unique: null, entityType: root.entityType, name: root.name } }]);
		this.#setCurrentLocation({ unique: null, entityType: root.entityType });
	}

	/**
	 * Maps an ancestors response to trail steps, cut off at the start node when there is one.
	 * @param {Array<UmbTreeItemModel>} items - The ancestors response.
	 * @returns {Array<UmbTreeItemPickerTrailStep>} The steps below the ceiling.
	 */
	#toSteps(items: Array<UmbTreeItemModel>): Array<UmbTreeItemPickerTrailStep> {
		let scoped = items;

		if (this.#startNode) {
			const ceilingIndex = items.findIndex((item) => item.unique === this.#startNode!.unique);
			scoped = ceilingIndex >= 0 ? items.slice(ceilingIndex) : items;
		}

		const steps = scoped.map((item) => ({
			crumb: { unique: item.unique, entityType: item.entityType, name: item.name },
			item,
		}));

		if (this.#startNode) return steps;

		const root = this.#trail.getValue().find((step) => step.crumb.unique === null);
		return root ? [root, ...steps] : steps;
	}

	#setTrail(steps: Array<UmbTreeItemPickerTrailStep>) {
		this.#trail.setValue(steps);
		this.#breadcrumb.setValue(steps.map((step) => step.crumb));
	}

	#setCurrentLocation(location: UmbTreeItemPickerLocation | undefined) {
		this.#currentLocation.setValue(location);
	}

	/**
	 * Remembers where the user browsed. Only `navigateTo` does this: loading the initial trail must not overwrite the
	 * location a previous session left behind, as that is what gets restored.
	 * @param {UmbTreeItemPickerLocation} location - The location the user browsed to.
	 */
	#remember(location: UmbTreeItemPickerLocation | undefined) {
		if (!this.#interactionMemoryManager) return;

		if (!location?.unique) {
			this.#interactionMemoryManager.deleteMemory(LOCATION_MEMORY_UNIQUE);
			return;
		}

		const memory: UmbInteractionMemoryModel = {
			unique: LOCATION_MEMORY_UNIQUE,
			value: { entity: { unique: location.unique, entityType: location.entityType } },
		};
		this.#interactionMemoryManager.setMemory(memory);
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
	}
}
