import { UmbContextProviderController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/*

TODO: We need to figure out if we like to keep using same alias for all workspace contexts.
If so we need to align on a interface that all of these implements. otherwise consumers cant trust the workspace-context.
*/
export abstract class UmbWorkspaceContext {

	protected _host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this._host = host;
		new UmbContextProviderController(host, 'umbWorkspaceContext', this);
	}

}
