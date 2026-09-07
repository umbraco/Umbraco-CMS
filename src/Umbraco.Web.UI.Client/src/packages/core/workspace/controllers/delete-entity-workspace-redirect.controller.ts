import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityDeletedEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UMB_PARENT_ENTITY_CONTEXT,
	type UmbEntityModel,
	type UmbParentEntityContext,
} from '@umbraco-cms/backoffice/entity';
import type { UmbEntityWorkspaceContext } from '../contexts/tokens/index.js';

export const UmbDeleteEntityWorkspaceRedirectControllerAlias = Symbol('UmbDeleteEntityWorkspaceRedirectControllerAlias');

export interface UmbDeleteEntityWorkspaceRedirectControllerArgs {
	/**
	 * Resolves the path to redirect to, given the deleted entity's parent — its real entity type and unique, which
	 * may differ from the deleted entity's own type (e.g. a folder) — or `undefined` when no parent is known (the
	 * deleted entity was at the root, or its parent couldn't be resolved), in which case the implementation decides
	 * where to send the user, e.g. the section or root workspace.
	 */
	getRedirectPath: (args: { entity: UmbEntityModel | undefined }) => string;
}

/**
 * Redirects the workspace to its parent once the open entity has been permanently deleted.
 */
export class UmbDeleteEntityWorkspaceRedirectController extends UmbControllerBase {
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#parentEntityContext?: UmbParentEntityContext;
	#workspaceContext: UmbEntityWorkspaceContext;
	#args: UmbDeleteEntityWorkspaceRedirectControllerArgs;

	/**
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbEntityWorkspaceContext} workspaceContext - The workspace context whose entity, once deleted, should trigger the redirect.
	 * @param {UmbDeleteEntityWorkspaceRedirectControllerArgs} args - The controller's configuration.
	 */
	constructor(host: UmbControllerHost, workspaceContext: UmbEntityWorkspaceContext, args: UmbDeleteEntityWorkspaceRedirectControllerArgs) {
		super(host, UmbDeleteEntityWorkspaceRedirectControllerAlias);
		this.#workspaceContext = workspaceContext;
		this.#args = args;

		this.consumeContext(UMB_PARENT_ENTITY_CONTEXT, (instance) => {
			this.#parentEntityContext = instance;
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;
			this.#actionEventContext?.removeEventListener(UmbEntityDeletedEvent.TYPE, this.#onDeleted);
			this.#actionEventContext?.addEventListener(UmbEntityDeletedEvent.TYPE, this.#onDeleted);
		});
	}

	#onDeleted = ((event: UmbEntityDeletedEvent) => {
		if (event.getUnique() !== this.#workspaceContext.getUnique()) return;
		if (event.getEntityType() !== this.#workspaceContext.getEntityType()) return;

		const entity = this.#parentEntityContext?.getParent();

		this.destroy();

		// The deleted entity's own URL is gone for good (unlike trash, which keeps a readonly URL reachable) —
		// replace it rather than push, so "back" doesn't land on a 404.
		window.history.replaceState(null, '', this.#args.getRedirectPath({ entity }));
	}) as EventListener;

	public override destroy(): void {
		// Remove the listener before tearing down the context consumer — destroying the consumer can itself
		// re-invoke its callback with `undefined`, which would otherwise clobber `#actionEventContext` first.
		this.#actionEventContext?.removeEventListener(UmbEntityDeletedEvent.TYPE, this.#onDeleted);
		super.destroy();
	}
}
