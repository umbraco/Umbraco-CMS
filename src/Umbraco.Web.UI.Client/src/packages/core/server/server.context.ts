import { UMB_SERVER_CONTEXT } from './server.context-token.js';
import type { UmbServerContextConfig } from './types.js';
import { RuntimeModeModel, ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import type { ServerInformationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { defer } from '@umbraco-cms/backoffice/external/rxjs';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbServerContext extends UmbContextBase {
	#serverUrl: string;
	#backofficePath: string;
	#serverConnection;

	#serverInformation = new UmbObjectState<ServerInformationResponseModel | undefined>(undefined);
	#serverInformationFetched = false;

	/**
	 * Observable that emits true when the server is running in Production mode,
	 * false when not in Production mode, or undefined until server information is loaded.
	 * UI consumers should treat undefined as restricted (safe default).
	 * The server information is fetched lazily on first subscription.
	 */
	public readonly isProductionMode = defer(() => {
		this.#ensureServerInformation();
		return this.#serverInformation.asObservablePart((info) =>
			info ? info.runtimeMode === RuntimeModeModel.PRODUCTION : undefined,
		);
	});

	/**
	 * Observable that emits true when the server is running in debug mode,
	 * false when not, or undefined until server information is loaded.
	 * The server information is fetched lazily on first subscription.
	 */
	public readonly isDebugMode = defer(() => {
		this.#ensureServerInformation();
		return this.#serverInformation.asObservablePart((info) => (info ? info.isDebugMode : undefined));
	});

	/**
	 * Observable that provides the full server information.
	 */
	public readonly serverInformation = this.#serverInformation.asObservable();

	constructor(host: UmbControllerHost, config: UmbServerContextConfig) {
		super(host, UMB_SERVER_CONTEXT);
		this.#serverUrl = config.serverUrl;
		this.#backofficePath = config.backofficePath;
		this.#serverConnection = config.serverConnection;
	}

	#ensureServerInformation() {
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
