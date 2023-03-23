// TODO: Be aware here we import a class from src!
import { UMB_ROUTE_CONTEXT_TOKEN } from '../router/route.context';
import type { UmbControllerInterface } from '../controller';
import { UmbModalRouteRegistration } from './modal-route-registration';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalConfig, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbControllerHostInterface } from 'libs/controller/controller-host.mixin';

export class UmbModalRouteRegistrationController<D extends object = object, R = any>
	extends UmbModalRouteRegistration<D, R>
	implements UmbControllerInterface
{
	//#host: UmbControllerHostInterface;

	#configuredPath: string;
	#uniqueParts: Map<string, string | undefined>;

	#routeContext?: typeof UMB_ROUTE_CONTEXT_TOKEN.TYPE;
	#modalRegistration?: UmbModalRouteRegistration;

	public get unique() {
		return undefined;
	}

	constructor(
		host: UmbControllerHostInterface,
		alias: UmbModalToken<D, R> | string,
		path: string,
		uniqueParts?: Map<string, string | undefined> | null,
		modalConfig?: UmbModalConfig
	) {
		super(alias, path, modalConfig);
		//this.#host = host;
		this.#configuredPath = path;
		this.#uniqueParts = uniqueParts || new Map();

		new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (_routeContext) => {
			this.#routeContext = _routeContext;
			this._registererModal();
		});
	}

	setUniqueIdentifier(identifier: string, value: string | undefined) {
		if (!this.#uniqueParts.has(identifier)) {
			throw new Error(
				`Identifier ${identifier} was not registered at the construction of the modal registration controller, it has to be.`
			);
		}
		this.#uniqueParts.set(identifier, value);
		this._registererModal();
	}

	private _registererModal() {
		if (!this.#routeContext) return;
		if (this.#modalRegistration) {
			this.#routeContext.unregisterModal(this.#modalRegistration);
			this.#modalRegistration = undefined;
		}

		const pathParts = Array.from(this.#uniqueParts.values());

		// Check if there is any undefined values of unique map:
		if (pathParts.some((value) => value === undefined)) return;

		// Add the configured part of the path:
		pathParts.push(this.#configuredPath);

		// Make this the path of the modal registration:
		this._setPath(pathParts.join('/'));

		this.#modalRegistration = this.#routeContext.registerModal(this);
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

	public destroy(): void {
		this.hostDisconnected();
	}
}
