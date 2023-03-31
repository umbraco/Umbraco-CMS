import { UmbContextToken } from '../token/context-token';
import { UmbContextProvider } from './context-provider';
import type { UmbControllerHostElement, UmbControllerInterface } from '@umbraco-cms/backoffice/controller';

export class UmbContextProviderController<T = unknown>
	extends UmbContextProvider<UmbControllerHostElement>
	implements UmbControllerInterface
{
	public get unique() {
		return this._contextAlias.toString();
	}

	constructor(host: UmbControllerHostElement, contextAlias: string | UmbContextToken<T>, instance: T) {
		super(host, contextAlias, instance);

		// If this API is already provided with this alias? Then we do not want to register this controller:
		const existingControllers = host.getControllers((x) => x.unique === this.unique);
		if (
			existingControllers.length > 0 &&
			(existingControllers[0] as UmbContextProviderController).providerInstance?.() === instance
		) {
			// Back out, this instance is already provided, by another controller.
			throw new Error(
				`Context API: The context of '${this.unique}' is already provided with the same API by another Context Provider Controller.`
			);
		} else {
			host.addController(this);
		}
	}

	public destroy() {
		super.destroy();
		if (this.host) {
			this.host.removeController(this);
		}
	}
}
