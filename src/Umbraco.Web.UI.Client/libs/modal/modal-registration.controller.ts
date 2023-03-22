import type { UmbRouteContext } from '../../src/core/router/route.context';
// TODO: Be aware here we import a class from src!
import { UMB_ROUTE_CONTEXT_TOKEN } from '../../src/core/router/route.context';
import { UmbModalRouteBuilder, UmbModalRouteOptions, UmbModalRouteRegistration } from './modal-route-registration';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbControllerHostInterface } from 'libs/controller/controller-host.mixin';
import { UmbController } from 'libs/controller/controller.class';

export type UmbModalRegistrationToken = UmbModalRouteRegistration;

export class UmbModalRegistrationController<D extends object = object, R = any> extends UmbController {
	#modalToken: UmbModalToken<D, R> | string;
	#modalOptions: UmbModalRouteOptions<D, R>;
	#routeContext?: UmbRouteContext;
	#modalRegistration?: UmbModalRegistrationToken;
	#uniqueParts;
	//#urlBuilder?: UmbModalRouteBuilder; // TODO: not going to work, as this will not trigger a re-render of the host element.

	constructor(
		host: UmbControllerHostInterface,
		alias: UmbModalToken<D, R> | string,
		options: UmbModalRouteOptions<D, R>,
		unique: Map<string, string | undefined> = new Map()
	) {
		super(host);
		this.#modalToken = alias;
		this.#modalOptions = {
			...options,
			/*getUrlBuilder: (urlBuilder) => {
				this.#urlBuilder = urlBuilder;
			},*/
		};
		this.#uniqueParts = unique;

		new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (_routeContext) => {
			this.#routeContext = _routeContext;
			this._registererModal();
		});
	}

	/*
	public getUrl(params: { [key: string]: string | number }) {
		return this.#urlBuilder?.(params) || null;
	}
	*/

	setUniqueIdentifier(identifier: string, value: string) {
		this.#uniqueParts.set(identifier, value);
	}

	private _registererModal() {
		if (!this.#routeContext) return;
		if (this.#modalRegistration) {
			this.#routeContext.unregisterModal(this.#modalRegistration);
		}

		const pathParts = Array.from(this.#uniqueParts.values());

		// Check if there is any undefined values of unique map:
		if (pathParts.some((value) => value === undefined)) return;

		// Add the configured part of the path:
		pathParts.push(this.#modalOptions.path);

		const modifiedModalOptions = {
			...this.#modalOptions,
			path: pathParts.join('/'),
		};

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
