import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
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

	#absolutePath?: string;
	#localPath?: string;

	/**
	 * Set the routes for the workspace.
	 * @param {Array<UmbRoute>} routes The routes for the workspace.
	 * @memberof UmbWorkspaceRouteManager
	 */
	setRoutes(routes: Array<UmbRoute>) {
		const allRoutes = [...routes] as Array<IComponentRoute>;

		if (routes.length > 0) {
			allRoutes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			});
		}

		const mappedRoutes = allRoutes.map((route) => {
			// override the setup method to set the active local path
			const oldSetupCallback = route.setup;

			route.setup = (_component: any, info: any) => {
				// TODO: could this be invalidated by the time it's used? [NL]
				// That would be a parent router switches path without the view being changed...
				this.#absolutePath = info.slot?.constructAbsolutePath();
				this.#localPath = info.match.fragments.consumed;

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
	 * @returns {string} The active local path.
	 * @memberof UmbWorkspaceRouteManager
	 */
	getActiveLocalPath(): string {
		return this.#localPath ?? '';
	}

	/**
	 * Get the absolute path.
	 * @returns {string | undefined} The absolute path.
	 * @memberof UmbWorkspaceRouteManager
	 */
	getAbsolutePath(): string | undefined {
		return this.#absolutePath;
	}
}
