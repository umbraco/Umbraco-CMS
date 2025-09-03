import type { UmbTreeItemChildrenManager } from './tree-item-children.manager.js';
import type { UmbTreeItemModel } from './types.js';
import type { UmbTargetPaginationManager } from '@umbraco-cms/backoffice/utils';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbBooleanState, type UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

interface UmbTreeItemTargetExpansionManagerArgs {
	childrenManager: UmbTreeItemChildrenManager<UmbTreeItemModel>;
	targetPaginationManager: UmbTargetPaginationManager;
}

export class UmbTreeItemTargetExpansionManager extends UmbControllerBase {
	#entity: UmbEntityModel | undefined;
	#observerController: UmbObserverController | undefined;
	#init?: Promise<unknown>;

	#childrenManager;
	#targetPaginationManager;

	#isExpanded = new UmbBooleanState(false);
	isExpanded = this.#isExpanded.asObservable();

	constructor(host: UmbControllerHost, args: UmbTreeItemTargetExpansionManagerArgs) {
		super(host);
		this.#childrenManager = args.childrenManager;
		this.#targetPaginationManager = args.targetPaginationManager;
	}

	setEntity(entity: UmbEntityModel | undefined) {
		this.#entity = entity;

		if (!entity) {
			this.#observerController?.destroy();
			return;
		}

		this.#observeExpansionForEntity(entity);
	}

	getEntity() {
		return this.#entity;
	}

	async #observeExpansionForEntity(entity: UmbEntityModel) {
		await this.#init;

		this.#observerController = this.observe(
			this.#treeContext?.expansion.entry(entity),
			async (entry) => {
				const isExpanded = entry !== undefined;

				const currentBaseTarget = this.#targetPaginationManager.getBaseTarget();
				const target = entry?.target;

				/* If a base target already exists (tree loaded to that point),
          don’t auto-reset when the target is removed.
          This happens when creating new items not yet in the tree. */
				if (currentBaseTarget && !target) {
					return;
				}

				/* If a new target is set we only want to reload children if the new target isn’t among the already loaded items. */
				const targetIsLoaded = this.#childrenManager.isChildLoaded(target);
				if (target && targetIsLoaded) {
					return;
				}

				// If we already have children and the target didn't change then we don't have to load new children
				const isNewTarget = target !== currentBaseTarget;
				if (isExpanded && this.#childrenManager.hasLoadedChildren() && !isNewTarget) {
					return;
				}

				// If this item is expanded and has children, load them
				if (isExpanded && this.#childrenManager.getHasChildren()) {
					this.#targetPaginationManager.setBaseTarget(target);
					this.#childrenManager.loadChildren();
				}

				this.#isExpanded.setValue(isExpanded);
			},
			'observeExpansion',
		);
	}
}
