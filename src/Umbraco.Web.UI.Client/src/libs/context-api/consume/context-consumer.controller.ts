import { UmbContextToken } from '../token/context-token.js';
import { UmbContextConsumer } from './context-consumer.js';
import { UmbContextCallback } from './context-request.event.js';
import type { UmbControllerHost, UmbController } from '@umbraco-cms/backoffice/controller-api';


export class UmbContextConsumerController<
	BaseType = unknown,
	DiscriminatedType extends BaseType = never,
	ResultType extends BaseType = keyof DiscriminatedType extends BaseType ? DiscriminatedType : BaseType
> extends UmbContextConsumer<BaseType, DiscriminatedType, ResultType> implements UmbController {
	#controllerAlias = Symbol();
	#host: UmbControllerHost;

	public get controllerAlias() {
		return this.#controllerAlias;
	}

	constructor(host: UmbControllerHost, contextAlias: string | UmbContextToken<BaseType, DiscriminatedType, ResultType>, callback: UmbContextCallback<ResultType>) {
		super(host.getHostElement(), contextAlias, callback);
		this.#host = host;
		host.addController(this);
	}

	public destroy() {
		super.destroy();
		this.#host.removeController(this);
	}
}
