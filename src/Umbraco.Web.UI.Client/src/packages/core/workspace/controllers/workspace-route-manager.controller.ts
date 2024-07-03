import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

export class UmbWorkspaceRouteManager extends UmbControllerBase {
	//
	#routes = new UmbArrayState<UmbRoute>([], (x) => x.path);
	public readonly routes = this.#routes.asObservable();

	setRoutes(routes: Array<UmbRoute>) {
		this.#routes.setValue([
			...routes,
			{
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
			},
		]);
	}
}
