import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { RuntimeLevelModel, ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbBooleanState, UmbNumberState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbServerConnection extends UmbControllerBase {
	#url: string;
	#status: RuntimeLevelModel = RuntimeLevelModel.UNKNOWN;

	#isConnected = new UmbBooleanState(false);
	isConnected = this.#isConnected.asObservable();

	#versionCheckPeriod = new UmbNumberState(undefined);
	versionCheckPeriod = this.#versionCheckPeriod.asObservable();

	#allowLocalLogin = new UmbBooleanState(false);
	allowLocalLogin = this.#allowLocalLogin.asObservable();

	#allowPasswordReset = new UmbBooleanState(false);
	allowPasswordReset = this.#allowPasswordReset.asObservable();

	constructor(host: UmbControllerHost, serverUrl: string) {
		super(host);
		this.#url = serverUrl;
	}

	/**
	 * Connects to the server.
	 * @memberof UmbServerConnection
	 */
	async connect() {
		await this.#setStatus();
		await this.#setServerConfiguration();
		return this;
	}

	/**
	 * Gets the URL of the server.
	 * @returns {*}
	 * @memberof UmbServerConnection
	 */
	getUrl() {
		return this.#url;
	}

	/**
	 * Gets the status of the server.
	 * @returns {string}
	 * @memberof UmbServerConnection
	 */
	getStatus() {
		if (!this.getIsConnected()) throw new Error('Server is not connected. Remember to await connect()');
		return this.#status;
	}

	/**
	 * Checks if the server is connected.
	 * @returns {boolean}
	 * @memberof UmbServerConnection
	 */
	getIsConnected() {
		return this.#isConnected.getValue();
	}

	async #setStatus() {
		const { data, error } = await tryExecute(this, ServerService.getServerStatus());
		if (error) {
			throw error;
		}

		this.#isConnected.setValue(true);
		this.#status = data?.serverStatus ?? RuntimeLevelModel.UNKNOWN;
	}

	async #setServerConfiguration() {
		const { data, error } = await tryExecute(this, ServerService.getServerConfiguration());
		if (error) {
			throw error;
		}

		this.#versionCheckPeriod.setValue(data?.versionCheckPeriod);
		this.#allowLocalLogin.setValue(data?.allowLocalLogin ?? false);
		this.#allowPasswordReset.setValue(data?.allowPasswordReset ?? false);
	}
}
