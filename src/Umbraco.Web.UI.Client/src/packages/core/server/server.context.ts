import { UMB_SERVER_CONTEXT } from './server.context-token.js';
import type { UmbServerContextConfig } from './types.js';
import { RuntimeModeModel, ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { ServerInformationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { defer, map } from '@umbraco-cms/backoffice/external/rxjs';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbServerContext extends UmbContextBase {
	#serverUrl: string;
	#backofficePath: string;
	#serverConnection;

	#serverInformation = new UmbObjectState<ServerInformationResponseModel | undefined>(undefined);
	#serverInformationFetched = false;

	/**
	 * Observable that provides the full server information.
	 * Every subscriber shares one request: the server is asked for its information at most once per
	 * app session, no matter how many consumers observe this or the derived `isProductionMode`.
	 */
	public readonly serverInformation = defer(() => {
		this.#requestServerInformation();
		return this.#serverInformation.asObservable();
	});

	/**
	 * Observable that emits true when the server is running in Production mode,
	 * false when not in Production mode, or undefined until server information is loaded.
	 * UI consumers should treat undefined as restricted (safe default).
	 */
	public readonly isProductionMode = this.serverInformation.pipe(
		map((info) => (info ? info.runtimeMode === RuntimeModeModel.PRODUCTION : undefined)),
	);

	constructor(host: UmbControllerHost, config: UmbServerContextConfig) {
		super(host, UMB_SERVER_CONTEXT);
		this.#serverUrl = config.serverUrl;
		this.#backofficePath = config.backofficePath;
		this.#serverConnection = config.serverConnection;
	}

	#requestServerInformation() {
		if (this.#serverInformationFetched) return;
		this.#serverInformationFetched = true;
		this.#fetchServerInformation();
	}

	async #fetchServerInformation() {
		const { data } = await tryExecute(this._host, ServerService.getServerInformation(), {
			disableNotifications: true,
		});
		if (data) {
			this.#serverInformation.setValue(data);
		} else {
			this.#serverInformationFetched = false;
			// Might need a retry mechanism here, but for now we just reset the flag so the next subscriber will trigger a new request. [NL]
		}
	}

	getBackofficePath() {
		return this.#backofficePath;
	}

	getServerUrl() {
		return this.#serverUrl;
	}

	getServerConnection() {
		return this.#serverConnection;
	}
}
