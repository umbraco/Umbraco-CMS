import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbStoreBase {
	constructor(protected _host: UmbControllerHostElement, public readonly storeAlias: string) {
		new UmbContextProviderController(_host, storeAlias, this);
	}
}
