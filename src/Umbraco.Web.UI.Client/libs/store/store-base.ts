import { UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbStoreBase {
	constructor(protected _host: UmbControllerHostInterface, public readonly storeAlias: string) {
		new UmbContextProviderController(_host, storeAlias, this);
	}
}
