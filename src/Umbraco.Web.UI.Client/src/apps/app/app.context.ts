import { UmbAppContextConfig } from './app-context-config.interface.js';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbAppContext extends UmbBaseController {
	#serverUrl: string;
	#backofficePath: string;

	constructor(host: UmbControllerHost, config: UmbAppContextConfig) {
		super(host);
		this.#serverUrl = config.serverUrl;
		this.#backofficePath = config.backofficePath;
		this.provideContext(UMB_APP, this);
	}

	getBackofficePath() {
		return this.#backofficePath;
	}

	getServerUrl() {
		return this.#serverUrl;
	}
}

export const UMB_APP = new UmbContextToken<UmbAppContext>('UMB_APP');
