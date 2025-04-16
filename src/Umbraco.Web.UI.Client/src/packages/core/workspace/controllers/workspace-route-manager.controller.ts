import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { IComponentRoute, UmbRoute } from '@umbraco-cms/backoffice/router';

/**
 * The workspace route manager.
 * @class UmbWorkspaceRouteManager
 * @augments {UmbControllerBase}
 */
export class UmbWorkspaceRouteManager extends UmbControllerBase {
	//
	#routes = new UmbArrayState<UmbRoute>([], (x) => x.path);
	public readonly routes = this.#routes.asObservable();

	#activeLocalPath = new UmbStringState('');

	/**
	 * Set the routes for the workspace.
	 * @param {Array<UmbRoute>} routes The routes for the workspace.
	 * @memberof UmbWorkspaceRouteManager
	 */
	setRoutes(routes: Array<UmbRoute>) {
		const allRoutes = [
			...routes,
			{
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			},
		] as Array<IComponentRoute>;

		const mappedRoutes = allRoutes.map((route) => {
			// override the setup method to set the active local path
			const oldSetupCallback = route.setup;

			route.setup = (_component: any, info: any) => {
				this.#activeLocalPath.setValue(info.match.fragments.consumed);

				if (oldSetupCallback) {
					oldSetupCallback(_component, info);
				}
			};

			return route;
		});

		this.#routes.setValue([...mappedRoutes]);
	}

	/**
	 * Get the routes for the workspace.
	 * @returns {Array<UmbRoute>} The routes for the workspace.
	 * @memberof UmbWorkspaceRouteManager
	 */
	getRoutes(): Array<UmbRoute> {
		return this.#routes.getValue();
	}

	/**
	 * Get the active local path.
	 * @returns {*}  {string}
	 * @memberof UmbWorkspaceRouteManager
	 */
	getActiveLocalPath(): string {
		return this.#activeLocalPath.getValue();
	}
}
