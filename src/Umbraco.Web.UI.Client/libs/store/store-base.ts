import { UmbContextProviderController } from "../context-api/provide/context-provider.controller";
import { UmbControllerHostInterface } from "../controller/controller-host.mixin";

export class UmbStoreBase {


	constructor (protected _host: UmbControllerHostInterface, public readonly storeAlias: string) {
		new UmbContextProviderController(_host, storeAlias, this);
	}

}
