import { UmbAppConfig } from './app-config.interface';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbAppContext {
	#backofficePath: string;

	constructor(config: UmbAppConfig) {
		this.#backofficePath = config.backofficePath;
	}

	getBackofficePath() {
		return this.#backofficePath;
	}
}

export const UMB_APP = new UmbContextToken<UmbAppContext>('UMB_APP');
