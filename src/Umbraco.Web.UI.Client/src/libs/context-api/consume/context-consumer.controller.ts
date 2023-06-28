import { UmbContextToken } from '../token/context-token.js';
import { UmbContextConsumer } from './context-consumer.js';
import { UmbContextCallback } from './context-request.event.js';
import type { UmbControllerHost, UmbController } from '@umbraco-cms/backoffice/controller-api';

export class UmbContextConsumerController<T = unknown> extends UmbContextConsumer<T> implements UmbController {
	#host: UmbControllerHost;

	public get unique() {
		return undefined;
	}

	constructor(host: UmbControllerHost, contextAlias: string | UmbContextToken<T>, callback: UmbContextCallback<T>) {
		super(host.getElement(), contextAlias, callback);
		this.#host = host;
		host.addController(this);
	}

	public destroy() {
		super.destroy();
		if (this.#host) {
			this.#host.removeController(this);
		}
	}
}
