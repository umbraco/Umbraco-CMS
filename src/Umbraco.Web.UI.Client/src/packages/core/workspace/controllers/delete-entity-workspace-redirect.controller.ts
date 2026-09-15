import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityDeletedEvent } from '@umbraco-cms/backoffice/entity-action';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export const UmbDeleteEntityWorkspaceRedirectControllerAlias = Symbol(
	'UmbDeleteEntityWorkspaceRedirectControllerAlias',
);

/**
 * The minimal shape this controller needs from a workspace context — intentionally not a public interface, since
 * it exists only to keep this controller's own dependency narrow, not as a contract for others to implement
 * against.
 */
type UmbDeleteEntityWorkspaceRedirectControllerWorkspaceContext = {
	getUnique(): UmbEntityUnique | undefined;
	getEntityType(): string;
	readonly navigationParentItemPath: Observable<string | undefined>;
};

/**
 * Redirects the workspace to its parent once the open entity has been permanently deleted, reading where to go
 * from the workspace context's own `navigationParentItemPath` — the one place that logic lives.
 */
export class UmbDeleteEntityWorkspaceRedirectController extends UmbControllerBase {
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#workspaceContext: UmbDeleteEntityWorkspaceRedirectControllerWorkspaceContext;
	#navigationParentItemPath?: string;

	/**
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @param {UmbDeleteEntityWorkspaceRedirectControllerWorkspaceContext} workspaceContext - The workspace context whose entity, once deleted, should trigger the redirect.
	 */
	constructor(host: UmbControllerHost, workspaceContext: UmbDeleteEntityWorkspaceRedirectControllerWorkspaceContext) {
		super(host, UmbDeleteEntityWorkspaceRedirectControllerAlias);
		this.#workspaceContext = workspaceContext;

		this.observe(workspaceContext.navigationParentItemPath, (path) => (this.#navigationParentItemPath = path), null);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;
			this.#actionEventContext?.removeEventListener(UmbEntityDeletedEvent.TYPE, this.#onDeleted);
			this.#actionEventContext?.addEventListener(UmbEntityDeletedEvent.TYPE, this.#onDeleted);
		});
	}

	#onDeleted = ((event: UmbEntityDeletedEvent) => {
		if (event.getUnique() !== this.#workspaceContext.getUnique()) return;
		if (event.getEntityType() !== this.#workspaceContext.getEntityType()) return;

		const path = this.#navigationParentItemPath;
		if (!path) return;

		// The deleted entity's own URL is gone for good (unlike trash, which keeps a readonly URL reachable) —
		// replace it rather than push, so "back" doesn't land on a 404.
		window.history.replaceState(null, '', path);
	}) as EventListener;

	public override destroy(): void {
		// Remove the listener before tearing down the context consumer — destroying the consumer can itself
		// re-invoke its callback with `undefined`, which would otherwise clobber `#actionEventContext` first.
		this.#actionEventContext?.removeEventListener(UmbEntityDeletedEvent.TYPE, this.#onDeleted);
		super.destroy();
	}
}
