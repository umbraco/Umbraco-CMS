import { ContextToken } from '../context-token';
import { UmbContextConsumer } from './context-consumer';
import { UmbContextCallback } from './context-request.event';

import { UmbControllerHostInterface, UmbControllerInterface } from '@umbraco-cms/controllers';

export class UmbContextConsumerController<T>
	extends UmbContextConsumer<T, UmbControllerHostInterface>
	implements UmbControllerInterface<T>
{
	public get unique() {
		return this._contextAlias;
	}

	constructor(
		host: UmbControllerHostInterface,
		contextAlias: string | ContextToken<T>,
		callback: UmbContextCallback<T>
	) {
		super(host, contextAlias, callback);
		host.addController(this);
	}

	public destroy() {
		if (this.host) {
			this.host.removeController(this);
		}
	}
}
