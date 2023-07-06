import { UmbWorkspaceContext } from '../workspace-context/index.js';
import { UmbBaseController, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityBase } from '@umbraco-cms/backoffice/models';
import { createRoutePathBuilder, type UmbRouterSlotElement } from '@umbraco-cms/backoffice/router';
import { ensurePathEndsWithSlash } from '@umbraco-cms/backoffice/utils';

/**
 * Observe the workspace context to see if the entity is new or not.
 * If that changes redirect to the edit url.
 *
 * This requires that the edit URL, is edit/:id
 * @param host
 * @param workspaceContext
 * @param router
 */
export class UmbWorkspaceIsNewRedirectController extends UmbBaseController {
	constructor(
		host: UmbControllerHost,
		workspaceContext: UmbWorkspaceContext<unknown, UmbEntityBase>,
		router: UmbRouterSlotElement
	) {
		super(host);

		// Navigate to edit route when language is created:
		this.observe(
			workspaceContext.isNew,
			(isNew) => {
				if (isNew === false) {
					const id = workspaceContext.getEntityId();
					if (router && id) {
						const routerPath = router.absoluteRouterPath;
						if (routerPath) {
							const newPath = createRoutePathBuilder(ensurePathEndsWithSlash(basePath) + 'edit/:id')({ id });
							window.history.pushState({}, '', newPath);

							this.destroy();
						}
					}
				}
			},
			'_observeIsNew'
		);
	}
}
