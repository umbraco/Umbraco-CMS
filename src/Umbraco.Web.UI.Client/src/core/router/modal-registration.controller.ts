import {
	UmbModalRegistrationToken,
	UmbModalRouteOptions,
	UmbRouteContext,
	UMB_ROUTE_CONTEXT_TOKEN,
} from './route.context';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbController, UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalToken } from '@umbraco-cms/modal';

export class UmbModalRegistrationController<D extends object = object, R = any> extends UmbController {
	#modalToken: UmbModalToken<D, R> | string;
	#modalOptions: UmbModalRouteOptions<D, R>;
	#routeContext?: UmbRouteContext;
	#modalRegistration?: UmbModalRegistrationToken;

	constructor(
		host: UmbControllerHostInterface,
		alias: UmbModalToken<D, R> | string,
		options: UmbModalRouteOptions<D, R>
	) {
		super(host);
		this.#modalToken = alias;
		this.#modalOptions = options;

		new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (_routeContext) => {
			this.#routeContext = _routeContext;
			this._registererModal();
		});
	}

	private _registererModal() {
		this.#modalRegistration = this.#routeContext?.registerModal(this.#modalToken, this.#modalOptions);
	}

	hostConnected() {
		if (!this.#modalRegistration) {
			this._registererModal();
		}
	}
	hostDisconnected(): void {
		if (this.#modalRegistration) {
			this.#routeContext?.unregisterModal(this.#modalRegistration);
			this.#modalRegistration = undefined;
		}
	}
}
