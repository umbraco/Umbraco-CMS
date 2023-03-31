import { UmbContextToken } from '../token/context-token';
import { UmbContextConsumer } from './context-consumer';
import { UmbContextCallback } from './context-request.event';
import type { UmbControllerHostElement, UmbControllerInterface } from '@umbraco-cms/backoffice/controller';

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
