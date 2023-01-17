import { ContextAlias } from '../context-token';
import { UmbContextConsumer } from './context-consumer';
import { UmbContextCallback } from './context-request.event';

import type { UmbControllerHostInterface, UmbControllerInterface } from '@umbraco-cms/controller';

export class UmbContextConsumerController<T = unknown>
	extends UmbContextConsumer<UmbControllerHostInterface, T>
	implements UmbControllerInterface
{
	public get unique() {
		return this._contextAlias?.toString();
	}

	constructor(
		host: UmbControllerHostInterface,
		contextAlias: string | ContextAlias<T>,
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
