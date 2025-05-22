import type { UmbContextToken } from '../token/index.js';
import type { UmbContextMinimal } from '../types.js';
import { UmbContextProvider } from './context-provider.js';
import type { UmbControllerHost, UmbController } from '@umbraco-cms/backoffice/controller-api';

export class UmbContextProviderController<
		BaseType extends UmbContextMinimal = UmbContextMinimal,
		ResultType extends BaseType = BaseType,
		InstanceType extends ResultType = ResultType,
	>
	extends UmbContextProvider<BaseType, ResultType>
	implements UmbController
{
	#host: UmbControllerHost;
	#controllerAlias: string;

	public get controllerAlias(): string {
		return this.#controllerAlias;
	}

	constructor(
		host: UmbControllerHost,
		contextAlias: string | UmbContextToken<BaseType, ResultType>,
		instance: InstanceType,
	) {
		super(host.getHostElement(), contextAlias, instance);
		this.#host = host;
		// Makes the controllerAlias unique for this instance, this enables multiple Contexts to be provided under the same name. (This only makes sense cause of Context Token Discriminators)
		// This does mean that if someone provides a context with the same name, but with a different instance, it will not override the previous instance. But its good since it enables extensions to provide contexts at the same scope of other contexts.
		this.#controllerAlias = contextAlias.toString() + '_' + (instance as any).constructor?.name;

		// If this API is already provided with this alias? Then we do not want to register this controller:
		const existingControllers = host.getUmbControllers((x) => x.controllerAlias === this.controllerAlias);
		if (
			existingControllers.length > 0 &&
			(existingControllers[0] as UmbContextProviderController).providerInstance?.() === instance
		) {
			// This just an additional awareness feature to make devs Aware, the alternative would be adding it anyway, but that would destroy existing controller of this alias.
			// Back out, this instance is already provided, by another controller.
			throw new Error(
				`Context API: The context of '${this.controllerAlias}' and instance '${
					(instance as any).constructor?.name ?? 'unnamed'
				}' is already provided by another Context Provider Controller.`,
			);
		} else {
			host.addUmbController(this);
		}
	}

	public override destroy(): void {
		if (this.#host) {
			const host = this.#host;
			(this.#host as unknown) = undefined;
			host.removeUmbController(this);
		}
		super.destroy();
	}
}
