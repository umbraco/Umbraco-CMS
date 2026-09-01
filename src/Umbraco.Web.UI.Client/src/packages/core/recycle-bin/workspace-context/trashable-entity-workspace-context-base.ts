import { UmbIsTrashedEntityContext } from '../contexts/is-trashed/is-trashed.entity-context.js';
import { UmbEntityRestoredFromRecycleBinEvent, UmbEntityTrashedEvent } from '../entity-action/index.js';
import type { UmbRecycleBinRepository } from '../recycle-bin-repository.interface.js';
import { UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT } from './trashable-entity-workspace.context-token.js';
import type { UmbTrashableEntityWorkspaceContext } from './types.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbVariantGuardRule } from '@umbraco-cms/backoffice/utils';

/**
 * Adds recycle-bin support (readonly-when-trashed, reload on trash/restore, redirect to parent when the current
 * user trashes the open entity) to any workspace context satisfying {@link UmbTrashableEntityWorkspaceContext}.
 *
 * Call {@link UmbTrashableEntityWorkspaceContextBase#_setRecycleBinRepositoryAlias} in the constructor of the subclass
 * to configure which recycle-bin repository to use.
 * @abstract
 * @class UmbTrashableEntityWorkspaceContextBase
 * @augments {UmbContextBase}
 */
export abstract class UmbTrashableEntityWorkspaceContextBase extends UmbContextBase {
	#recycleBinRepositoryAlias?: string;
	#workspaceContext?: UmbTrashableEntityWorkspaceContext;
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, 'UmbTrashableEntityWorkspaceContext');

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

	protected _setRecycleBinRepositoryAlias(alias: string) {
		this.#recycleBinRepositoryAlias = alias;
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
		this.#workspaceContext?.reload();

		const unique = event.getUnique();
		if (unique) {
			this.#redirectToParent(unique);
		}
	};

	async #redirectToParent(unique: string) {
		if (!this.#workspaceContext) return;

		// Don't redirect a workspace hosted in a modal (e.g. opened from a picker) — it should stay put, readonly.
		if (this.#workspaceContext.modalContext) return;

		if (!this.#recycleBinRepositoryAlias) throw new Error('Recycle bin repository alias is not set');
		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.#recycleBinRepositoryAlias,
		);

		const { data } = await recycleBinRepository.requestOriginalParent({ unique });
		const parentUnique = data?.unique ?? null;
		const entityType = this.#workspaceContext.getEntityType();

		if (parentUnique) {
			// replaceState: staying within the same edit/:unique route, whose own setup() re-triggers on URL change
			// and loads the new unique — an inaccessible parent falls through to the normal forbidden/not-found state.
			window.history.replaceState(null, '', this.getRedirectPath({ entity: { entityType, unique: parentUnique } }));
			return;
		}

		// pushState: no parent means leaving the workspace entirely for a different top-level route, so this is a
		// new history entry rather than replacing the current one.
		window.history.pushState(null, '', this.getRedirectPath({ entity: { entityType, unique: null } }));
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
