import type { UmbTreeRepository } from '../data/tree-repository.interface.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

/**
 * Arguments for {@link UmbTreePlacementReloadController.reload}.
 * @internal
 */
export interface UmbTreePlacementReloadArgs {
	/**
	 * The tree repository used to look up the destination. Its lifecycle stays with the caller.
	 */
	treeRepository: UmbTreeRepository | undefined;
	/**
	 * The entity the action was performed on, whose location is reloaded.
	 */
	source: { unique: string; entityType: string };
	/**
	 * The entity that now sits in the destination: the moved entity itself, or the newly created copy.
	 * The destination is its parent. When it is not known, only the source location is reloaded.
	 */
	placedEntity: { unique: string; entityType: string } | undefined;
}

// TODO: If the tree picker modal value included the entity type of the picked items, not only their uniques, the picked destination could be reloaded directly, saving the request for the parent of the placed entity. [MR]
/**
 * Reloads the tree locations affected by placing an entity in a new location: the location of the source and the destination.
 * A single-use controller, destroyed once the reload has been requested.
 * @internal
 */
export class UmbTreePlacementReloadController extends UmbControllerBase {
	/**
	 * Requests a reload of the source location and the destination.
	 * @param {UmbTreePlacementReloadArgs} args - The tree repository, the source and the placed entity.
	 */
	async reload(args: UmbTreePlacementReloadArgs) {
		try {
			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			if (!actionEventContext) throw new Error('Action event context is not available');

			// Resolve before dispatching, as reloading the source location can destroy the element hosting the action.
			const destination = await this.#requestParent(args.treeRepository, args.placedEntity);

			actionEventContext.dispatchEvent(new UmbRequestReloadStructureForEntityEvent(args.source));

			if (!destination) return;

			actionEventContext.dispatchEvent(new UmbRequestReloadChildrenOfEntityEvent(destination));

			// A structure reload of the root would reload the whole tree, which the children reload already covers.
			if (destination.unique !== null) {
				actionEventContext.dispatchEvent(new UmbRequestReloadStructureForEntityEvent(destination));
			}
		} finally {
			this.destroy();
		}
	}

	async #requestParent(
		treeRepository: UmbTreeRepository | undefined,
		entity: UmbTreePlacementReloadArgs['placedEntity'],
	): Promise<UmbEntityModel | undefined> {
		if (!treeRepository || !entity) return undefined;

		try {
			const { data } = await treeRepository.requestTreeItemAncestors({ treeItem: entity });
			const parent = data?.find((item) => item.unique === entity.unique)?.parent;
			return parent ? { entityType: parent.entityType, unique: parent.unique } : undefined;
		} catch {
			return undefined;
		}
	}
}
