import { UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export abstract class UmbWorkspaceContext {

	protected _host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this._host = host;
		new UmbContextProviderController(host, 'UmbWorkspaceContext', this);
	}

}
