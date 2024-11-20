import { RuntimeLevelModel, ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbServerConnection {
	#url: string;
	#status: RuntimeLevelModel = RuntimeLevelModel.UNKNOWN;

	#isConnected = new UmbBooleanState(false);
	isConnected = this.#isConnected.asObservable();

	constructor(serverUrl: string) {
		this.#url = serverUrl;
	}

	/**
	 * Connects to the server.
	 * @memberof UmbServerConnection
	 */
	async connect() {
		await this.#setStatus();
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
		const { data, error } = await tryExecute(ServerService.getServerStatus());
		if (error) {
			throw error;
		}

		this.#isConnected.setValue(true);
		this.#status = data?.serverStatus ?? RuntimeLevelModel.UNKNOWN;
	}
}
