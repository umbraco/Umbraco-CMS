import type { UmbRoutePathAddendum } from './route-path-addendum.interface.js';
import { UMB_ROUTE_PATH_ADDENDUM_CONTEXT } from './route-path-addendum.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRoutePathAddendumResetContext extends UmbContextBase implements UmbRoutePathAddendum {
	#appendum = new UmbStringState('');
	readonly addendum = this.#appendum.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ROUTE_PATH_ADDENDUM_CONTEXT);
	}
}
