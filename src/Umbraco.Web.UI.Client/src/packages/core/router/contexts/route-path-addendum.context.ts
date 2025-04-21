import type { UmbRoutePathAddendum } from './route-path-addendum.interface.js';
import { UMB_ROUTE_PATH_ADDENDUM_CONTEXT } from './route-path-addendum.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRoutePathAddendumContext
	extends UmbContextBase<UmbRoutePathAddendum, typeof UMB_ROUTE_PATH_ADDENDUM_CONTEXT>
	implements UmbRoutePathAddendum
{
	#parent?: string;
	#current?: string;

	#addendum = new UmbStringState(undefined);
	readonly addendum = this.#addendum.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ROUTE_PATH_ADDENDUM_CONTEXT);

		this.consumeContext(UMB_ROUTE_PATH_ADDENDUM_CONTEXT, (context) => {
			this.observe(context?.addendum, (addendum) => {
				this.#parent = addendum;
				this.#update();
			});
		}).skipHost();
	}

	setAddendum(addendum: string | undefined) {
		this.#current = addendum;
		this.#update();
	}

	#update() {
		if (this.#parent === undefined || this.#current === undefined) {
			return;
		}
		// if none of the strings are empty strings, then we should add a slash in front of the currentAddendum. So we get one in between.
		const add = this.#current === '' || this.#parent === '' ? this.#current : '/' + this.#current;
		this.#addendum.setValue(this.#parent + add);
	}
}
