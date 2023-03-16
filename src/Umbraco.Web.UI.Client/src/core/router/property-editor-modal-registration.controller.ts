import { UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN } from '../../backoffice/shared/components/workspace-property/workspace-property.context';
import {
	UmbModalRegistrationToken,
	UmbModalRouteOptions,
	UmbRouteContext,
	UMB_ROUTE_CONTEXT_TOKEN,
} from './route.context';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbController, UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalToken } from '@umbraco-cms/modal';
import { UmbObserverController } from '@umbraco-cms/observable-api';

export class UmbPropertyEditorModalRegistrationController<D extends object = object, R = any> extends UmbController {
	#modalToken: UmbModalToken<D, R> | string;
	#modalOptions: UmbModalRouteOptions<D, R>;
	#routeContext?: UmbRouteContext;
	#propertyAlias?: string;
	#variantId?: string;
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

		new UmbContextConsumerController(host, UMB_WORKSPACE_PROPERTY_CONTEXT_TOKEN, (_routeContext) => {
			new UmbObserverController(host, _routeContext.alias, (alias) => {
				this.#propertyAlias = alias;
				this._registererModal();
			});
			new UmbObserverController(host, _routeContext.variantId, (variantId) => {
				this.#variantId = variantId?.toString();
				this._registererModal();
			});
		});
	}

	private _registererModal() {
		console.log('try registrer');
		if (!this.#routeContext || !this.#propertyAlias || !this.#variantId) return;
		if (this.#modalRegistration) {
			this.#routeContext.unregisterModal(this.#modalRegistration);
		}

		const modifiedModalOptions = {
			...this.#modalOptions,
			path: `${this.#propertyAlias}/${this.#variantId}/${this.#modalOptions.path}`,
		};

		console.log(this.#modalOptions);
		console.log(modifiedModalOptions);

		this.#modalRegistration = this.#routeContext?.registerModal(this.#modalToken, modifiedModalOptions);
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
