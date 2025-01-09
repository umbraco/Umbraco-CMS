import type { UmbRoutePathAddendum } from './route-path-addendum.interface.js';
import { UMB_ROUTE_PATH_ADDENDUM_CONTEXT } from './route-path-addendum.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRoutePathAddendumContext
	extends UmbContextBase<UmbRoutePathAddendum, typeof UMB_ROUTE_PATH_ADDENDUM_CONTEXT>
	implements UmbRoutePathAddendum
{
	#parentAddendum?: string;
	#currentAddendum?: string;

	#pathAddendum = new UmbStringState(undefined);
	readonly addendum = this.#pathAddendum.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ROUTE_PATH_ADDENDUM_CONTEXT);

		this.consumeContext(UMB_ROUTE_PATH_ADDENDUM_CONTEXT, (context) => {
			this.observe(context.addendum, (addendum) => {
				this.#parentAddendum = addendum;
				this.#update();
			});
		}).skipHost();
	}

	setAddendum(addendum: string) {
		this.#currentAddendum = addendum;
		this.#update();
	}

	#update() {
		if (this.#parentAddendum === undefined || this.#currentAddendum === undefined) {
			return;
		}
		const base = this.#parentAddendum === '' ? this.#parentAddendum : this.#parentAddendum + '/';
		this.#pathAddendum.setValue(base + this.#currentAddendum);
	}
}
