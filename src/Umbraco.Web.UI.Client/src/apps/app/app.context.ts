import { UmbAppContextConfig } from './app-context-config.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbAppContext {
	#serverUrl: string;
	#backofficePath: string;

	constructor(config: UmbAppContextConfig) {
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

export const UMB_APP = new UmbContextToken<UmbAppContext>('UMB_APP');
