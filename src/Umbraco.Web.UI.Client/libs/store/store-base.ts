import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

// TODO: Make a Store interface?
export class UmbStoreBase {
	constructor(protected _host: UmbControllerHostElement, public readonly storeAlias: string) {
		new UmbContextProviderController(_host, storeAlias, this);
	}
}
