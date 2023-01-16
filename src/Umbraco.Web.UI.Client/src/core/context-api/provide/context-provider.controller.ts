import { UmbContextProvider } from './context-provider';
import type { UmbControllerInterface } from 'src/core/controller/controller.interface';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export class UmbContextProviderController extends UmbContextProvider<UmbControllerHostInterface> implements UmbControllerInterface {

	public get unique() {
		return this._contextAlias;
	}

	constructor(host:UmbControllerHostInterface, contextAlias: string, instance: unknown) {
		super(host, contextAlias, instance);

		// TODO: What if this API is already provided with this alias? maybe handle this in the controller:
		// TODO: Remove/destroy existing controller of same alias.

		host.addController(this);
	}

	public destroy() {
		if (this.host) {
			this.host.removeController(this);
		}
	}

}
