import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

/**
 * The workspace route manager.
 * @class UmbWorkspaceRouteManager
 * @augments {UmbControllerBase}
 */
export class UmbWorkspaceRouteManager extends UmbControllerBase {
	//
	#routes = new UmbArrayState<UmbRoute>([], (x) => x.path);
	public readonly routes = this.#routes.asObservable();

	/**
	 * Set the routes for the workspace.
	 * @param {Array<UmbRoute>} routes The routes for the workspace.
	 * @memberof UmbWorkspaceRouteManager
	 */
	setRoutes(routes: Array<UmbRoute>) {
		this.#routes.setValue([
			...routes,
			{
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			},
		]);
	}

	/**
	 * Get the routes for the workspace.
	 * @returns {Array<UmbRoute>} The routes for the workspace.
	 * @memberof UmbWorkspaceRouteManager
	 */
	getRoutes(): Array<UmbRoute> {
		return this.#routes.getValue();
	}
}
