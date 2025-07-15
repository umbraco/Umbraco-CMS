import { UmbServerFileRenamedEntityEvent } from '../event/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbRouterSlotElement } from '@umbraco-cms/backoffice/router';
import { ensurePathEndsWithSlash, umbUrlPatternToString } from '@umbraco-cms/backoffice/utils';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbSubmittableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';

export const UmbServerFileRenameWorkspaceRedirectControllerAlias = Symbol(
	'ServerFileRenameWorkspaceRedirectControllerAlias',
);

export class UmbServerFileRenameWorkspaceRedirectController extends UmbControllerBase {
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;
	#workspaceContext: UmbSubmittableWorkspaceContextBase<unknown>;
	#router: UmbRouterSlotElement;

	constructor(
		host: UmbControllerHost,
		workspaceContext: UmbSubmittableWorkspaceContextBase<unknown>,
		router: UmbRouterSlotElement,
	) {
		super(host, UmbServerFileRenameWorkspaceRedirectControllerAlias);

		this.#workspaceContext = workspaceContext;
		this.#router = router;

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#actionEventContext = context;

			if (this.#actionEventContext) {
				this.#actionEventContext.removeEventListener(UmbServerFileRenamedEntityEvent.TYPE, this.#onFileRenamed);
				this.#actionEventContext.addEventListener(UmbServerFileRenamedEntityEvent.TYPE, this.#onFileRenamed);
			}
		});
	}

	#onFileRenamed = ((event: UmbServerFileRenamedEntityEvent) => {
		if (!this.#router) throw new Error('Router is required for this controller.');

		// Don't redirect if the event is not for the current entity
		const currentUnique = this.#workspaceContext.getUnique();
		const eventUnique = event.getUnique();
		if (currentUnique !== eventUnique) return;

		const newUnique = event.getNewUnique();
		if (!newUnique) throw new Error('New unique is required for this event.');

		const routerPath = this.#router.absoluteRouterPath;
		if (!routerPath) throw new Error('Router path is required for this controller.');

		const newPath: string = umbUrlPatternToString(ensurePathEndsWithSlash(routerPath) + 'edit/:unique', {
			unique: newUnique,
		});

		this.destroy();
		window.history.replaceState(null, '', newPath);
	}) as EventListener;

	public override destroy(): void {
		super.destroy();
		this.#actionEventContext?.removeEventListener(UmbServerFileRenamedEntityEvent.TYPE, this.#onFileRenamed);
	}
}
