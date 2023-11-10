import { RuntimeLevelModel, ServerResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbServerConnection {
	#serverUrl: string;
	#serverStatus: RuntimeLevelModel = RuntimeLevelModel.UNKNOWN;
	#connected = new UmbBooleanState(false);

	constructor(serverUrl: string) {
		this.#serverUrl = serverUrl;
	}

	async connect() {
		await this.#setStatus();
	}

	getStatus() {
		return this.#serverStatus;
	}

	async #setStatus() {
		const { data, error } = await tryExecute(ServerResource.getServerStatus());
		if (error) {
			throw error;
		}

		this.#connected.next(true);
		this.#serverStatus = data?.serverStatus ?? RuntimeLevelModel.UNKNOWN;
	}
}
