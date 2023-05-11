// TODO: Be aware here we import a class from src!
import { UMB_ROUTE_CONTEXT_TOKEN } from '../router/route.context';
import { UmbModalRouteRegistration } from './modal-route-registration';
import type { UmbControllerHostElement, UmbControllerInterface } from '@umbraco-cms/backoffice/controller';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalConfig, UmbModalToken } from '@umbraco-cms/backoffice/modal';

export class UmbModalRouteRegistrationController<D extends object = object, R = any>
	extends UmbModalRouteRegistration<D, R>
	implements UmbControllerInterface
{
	//#host: UmbControllerHostInterface;

	#additionalPath: string | null;
	#uniquePaths: Map<string, string | undefined> = new Map();

	#routeContext?: typeof UMB_ROUTE_CONTEXT_TOKEN.TYPE;
	#modalRegistration?: UmbModalRouteRegistration;

	public get unique() {
		return undefined;
	}

	constructor(
		host: UmbControllerHostElement,
		alias: UmbModalToken<D, R> | string,
		additionalPath: string | null = null,
		uniquePaths?: Array<string> | null,
		modalConfig?: UmbModalConfig
	) {
		super(alias, null, modalConfig);
		//this.#host = host;
		this.#additionalPath = additionalPath;
		if (uniquePaths) {
			uniquePaths.forEach((name) => {
				this.#uniquePaths.set(name, undefined);
			});
		}

		new UmbContextConsumerController(host, UMB_ROUTE_CONTEXT_TOKEN, (_routeContext) => {
			this.#routeContext = _routeContext;
			this._registererModal();
		});
	}

	setUniquePathValue(identifier: string, value: string | undefined) {
		if (!this.#uniquePaths.has(identifier)) {
			throw new Error(
				`Identifier ${identifier} was not registered at the construction of the modal registration controller, it has to be.`
			);
		}
		this.#uniquePaths.set(identifier, value);
		this._registererModal();
	}

	private _registererModal() {
		if (!this.#routeContext) return;
		if (this.#modalRegistration) {
			this.#routeContext.unregisterModal(this.#modalRegistration);
			this.#modalRegistration = undefined;
		}

		const pathParts = Array.from(this.#uniquePaths.values());

		// Check if there is any undefined values of unique map:
		if (pathParts.some((value) => value === undefined)) return;

		if (this.#additionalPath) {
			// Add the configured part of the path:
			pathParts.push(this.#additionalPath);
		}

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
