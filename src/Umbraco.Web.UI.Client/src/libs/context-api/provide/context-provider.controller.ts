import { UmbContextToken } from '../token/context-token.js';
import { UmbContextProvider } from './context-provider.js';
import type { UmbControllerHost, UmbController } from '@umbraco-cms/backoffice/controller-api';

export class UmbContextProviderController<
	BaseType = unknown,
	DiscriminatorType extends BaseType = never,
	ResultType extends BaseType = keyof DiscriminatorType extends BaseType ? DiscriminatorType : BaseType
> extends UmbContextProvider<BaseType, DiscriminatorType, ResultType> implements UmbController {
	#host: UmbControllerHost;

	public get controllerAlias() {
		return this._contextAlias.toString();
	}

	constructor(host: UmbControllerHost, contextAlias: string | UmbContextToken<BaseType, DiscriminatorType, ResultType>, instance: ResultType) {
		super(host.getHostElement(), contextAlias, instance);
		this.#host = host;

		// If this API is already provided with this alias? Then we do not want to register this controller:
		const existingControllers = host.getControllers((x) => x.controllerAlias === this.controllerAlias);
		if (
			existingControllers.length > 0 &&
			(existingControllers[0] as UmbContextProviderController).providerInstance?.() === instance
		) {
			// Back out, this instance is already provided, by another controller.
			throw new Error(
				`Context API: The context of '${this.controllerAlias}' is already provided with the same API by another Context Provider Controller.`
			);
		} else {
			host.addController(this);
		}
	}

	public destroy() {
		if (this.#host) {
			this.#host.removeController(this);
		}
		super.destroy();
	}
}
