import type { UmbAppContextConfig } from './app-context-config.interface.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbAppContext extends UmbContextBase<UmbAppContext> {
	#serverUrl: string;
	#backofficePath: string;

	constructor(host: UmbControllerHost, config: UmbAppContextConfig) {
		super(host, UMB_APP_CONTEXT);
		this.#serverUrl = config.serverUrl;
		this.#backofficePath = config.backofficePath;
	}

	getBackofficePath() {
		return this.#backofficePath;
	}

	getServerUrl() {
		return this.#serverUrl;
	}
}

export const UMB_APP_CONTEXT = new UmbContextToken<UmbAppContext>('UmbAppContext');
