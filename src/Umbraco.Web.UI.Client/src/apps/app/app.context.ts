import { UmbAppContextConfig } from './app-context-config.interface';
import { UmbContextToken } from 'src/libs/context-api';

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
