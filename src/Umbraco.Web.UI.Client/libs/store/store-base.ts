import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

export class UmbStoreBase {
	constructor(protected _host: UmbControllerHostInterface, public readonly storeAlias: string) {
		new UmbContextProviderController(_host, storeAlias, this);
	}
}
