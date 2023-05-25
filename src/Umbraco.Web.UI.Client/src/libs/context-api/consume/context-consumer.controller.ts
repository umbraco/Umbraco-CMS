import { UmbContextToken } from '../token/context-token.js';
import { UmbContextConsumer } from './context-consumer.js';
import { UmbContextCallback } from './context-request.event.js';
import type { UmbControllerHostElement, UmbControllerInterface } from '@umbraco-cms/backoffice/controller-api';

export class UmbContextConsumerController<T = unknown>
	extends UmbContextConsumer<UmbControllerHostElement, T>
	implements UmbControllerInterface
{
	public get unique() {
		return undefined;
	}

	constructor(
		host: UmbControllerHostElement,
		contextAlias: string | UmbContextToken<T>,
		callback: UmbContextCallback<T>
	) {
		super(host, contextAlias, callback);
		host.addController(this);
	}

	public destroy() {
		super.destroy();
		if (this.host) {
			this.host.removeController(this);
		}
	}
}
