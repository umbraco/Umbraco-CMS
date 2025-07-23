import type { UmbSubmittableWorkspaceContextBase } from '../submittable/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbRouterSlotElement } from '@umbraco-cms/backoffice/router';
import { ensurePathEndsWithSlash, umbUrlPatternToString } from '@umbraco-cms/backoffice/utils';

export const UmbWorkspaceIsNewRedirectControllerAlias = Symbol('IsNewRedirectControllerAlias');

/**
 * Observe the workspace context to see if the entity is new or not.
 * If that changes redirect to the edit url.
 *
 * This requires that the edit URL, is edit/:id
 * @param host
 * @param workspaceContext
 * @param router
 */
export class UmbWorkspaceIsNewRedirectController extends UmbControllerBase {
	/**
	 * TODO: Figure out why we need this timeout. [NL]
	 * The problem this fixes it when save & publishing a document open in a modal, like from a collection.
	 * The redirect triggers something that ends up re-setting the path, making the modal path the one in the browser despite the modal is closed.
	 */
	timeout: any | undefined;

	constructor(
		host: UmbControllerHost,
		workspaceContext: UmbSubmittableWorkspaceContextBase<unknown>,
		router: UmbRouterSlotElement,
	) {
		super(host, UmbWorkspaceIsNewRedirectControllerAlias);

		// Navigate to edit route when language is created:
		this.observe(workspaceContext.isNew, (isNew) => {
			if (this.timeout) {
				clearTimeout(this.timeout);
			}
			if (isNew === false) {
				this.timeout = setTimeout(() => {
					const unique = workspaceContext.getUnique();
					if (router && unique) {
						const routerPath = router.absoluteRouterPath;
						if (routerPath) {
							const newPath: string = umbUrlPatternToString(ensurePathEndsWithSlash(routerPath) + 'edit/:id', {
								id: unique,
							});
							this.destroy();
							// get current url:
							const currentUrl = window.location.href;
							if (
								router.localActiveViewPath === undefined ||
								router.localActiveViewPath === '' ||
								!currentUrl.includes(router.localActiveViewPath)
							) {
								return;
							}
							// Check that we are still part of the DOM and thereby relevant:
							window.history.replaceState(null, '', newPath);
						}
					}
					this.timeout = undefined;
				}, 500);
			}
		});

		// TODO: If workspace route changes cause of other reasons then this controller should be destroyed.
	}

	override destroy() {
		super.destroy();
		if (this.timeout) {
			clearTimeout(this.timeout);
			this.timeout = undefined;
		}
	}
}
