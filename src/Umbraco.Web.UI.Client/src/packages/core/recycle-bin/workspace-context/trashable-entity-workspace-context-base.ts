import { UmbIsTrashedEntityContext } from '../contexts/is-trashed/is-trashed.entity-context.js';
import { UmbEntityRestoredFromRecycleBinEvent, UmbEntityTrashedEvent } from '../entity-action/index.js';
import { UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT } from './trashable-entity-workspace.context-token.js';
import type { UmbTrashableEntityWorkspaceContext } from './types.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_PARENT_ENTITY_CONTEXT,
	type UmbEntityModel,
	type UmbParentEntityContext,
} from '@umbraco-cms/backoffice/entity';
import type { UmbVariantGuardRule } from '@umbraco-cms/backoffice/utils';

/**
 * Adds recycle-bin support (readonly-when-trashed, reload on trash/restore, redirect to parent when the current
 * user trashes the open entity) to any workspace context satisfying {@link UmbTrashableEntityWorkspaceContext}.
 * @abstract
 * @class UmbTrashableEntityWorkspaceContextBase
 * @augments {UmbContextBase}
 */
export abstract class UmbTrashableEntityWorkspaceContextBase extends UmbContextBase {
	#workspaceContext?: UmbTrashableEntityWorkspaceContext;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#isTrashedContext = new UmbIsTrashedEntityContext(this);
	#parentEntityContext?: UmbParentEntityContext;

	constructor(host: UmbControllerHost) {
		super(host, 'UmbTrashableEntityWorkspaceContext');

		this.consumeContext(UMB_PARENT_ENTITY_CONTEXT, (instance) => {
			this.#parentEntityContext = instance;
		});

		this.consumeContext(UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;

			this.observe(
				this.#workspaceContext?.isTrashed,
				(isTrashed) => this.#onTrashStateChange(isTrashed),
				'umbRecycleBinObserveIsTrashed',
			);

			this.observe(
				this.#workspaceContext?.isNew,
				(isNew) => {
					if (isNew) {
						this.#isTrashedContext.setIsTrashed(false);
					}
				},
				'umbRecycleBinObserveIsNew',
			);
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (actionEventContext) => {
			this.#removeEventListeners();
			this.#actionEventContext = actionEventContext;
			this.#addEventListeners();
		});
	}

	/**
	 * The path to redirect the workspace to once the open entity has been trashed.
	 * @param {object} args - The redirect args.
	 * @param {UmbEntityModel} args.entity - The entity type of the trashed entity (i.e. this workspace's own entity
	 * type), and the unique of the original parent to redirect to — or a `null` unique if the trashed entity had
	 * no parent (was at the root), in which case the implementation decides where to send the user, e.g. the
	 * section root.
	 */
	protected abstract getRedirectPath(args: { entity: UmbEntityModel }): string;

	#addEventListeners() {
		this.#actionEventContext?.addEventListener(UmbEntityTrashedEvent.TYPE, this.#onTrashed as EventListener);
		this.#actionEventContext?.addEventListener(
			UmbEntityRestoredFromRecycleBinEvent.TYPE,
			this.#onRestored as EventListener,
		);
	}

	#removeEventListeners() {
		this.#actionEventContext?.removeEventListener(UmbEntityTrashedEvent.TYPE, this.#onTrashed as EventListener);
		this.#actionEventContext?.removeEventListener(
			UmbEntityRestoredFromRecycleBinEvent.TYPE,
			this.#onRestored as EventListener,
		);
	}

	#isMatchingEvent(event: UmbEntityTrashedEvent | UmbEntityRestoredFromRecycleBinEvent) {
		return (
			event.getUnique() === this.#workspaceContext?.getUnique() &&
			event.getEntityType() === this.#workspaceContext?.getEntityType()
		);
	}

	#onRestored = (event: UmbEntityRestoredFromRecycleBinEvent) => {
		if (!this.#isMatchingEvent(event)) return;
		this.#workspaceContext?.reload();
	};

	#onTrashed = (event: UmbEntityTrashedEvent) => {
		if (!this.#isMatchingEvent(event)) return;

		// Only reload when staying put (a modal), to refresh the visible trashed state — when redirecting away,
		// the new data isn't needed here.
		if (event.getUnique() && !this.#workspaceContext?.modalContext) {
			try {
				this.#redirectToParent();
			} catch (error) {
				console.error('Failed to redirect after trashing, reloading in place instead:', error);
				this.#workspaceContext?.reload();
			}
			return;
		}

		this.#workspaceContext?.reload();
	};

	#redirectToParent() {
		if (!this.#workspaceContext) return;

		const entityType = this.#workspaceContext.getEntityType();
		const parentUnique = this.#parentEntityContext?.getParent()?.unique ?? null;

		// Trashing doesn't delete the entity — it stays reachable, readonly, at its own edit URL. So unlike a
		// delete or a rename, that URL is still worth keeping in history: pushState rather than replaceState.
		window.history.pushState(null, '', this.getRedirectPath({ entity: { entityType, unique: parentUnique } }));
	}

	#onTrashStateChange(isTrashed?: boolean) {
		this.#isTrashedContext.setIsTrashed(isTrashed ?? false);

		const guardUnique = `UMB_PREVENT_EDIT_TRASHED_ITEM`;

		if (!isTrashed) {
			this.#workspaceContext?.readOnlyGuard.removeRule(guardUnique);
			return;
		}

		const rule: UmbVariantGuardRule = {
			unique: guardUnique,
			permitted: true,
		};

		// TODO: Change to use property write guard when it supports making the name read-only.
		this.#workspaceContext?.readOnlyGuard.addRule(rule);
	}

	public override destroy(): void {
		this.#removeEventListeners();
		super.destroy();
	}
}
