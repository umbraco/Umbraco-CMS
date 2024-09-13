import type { UmbServerConfiguration, UmbServerUpgradeCheck } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute, tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbSysinfoRepository extends UmbRepositoryBase {
	#serverConfiguration?: UmbServerConfiguration;

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Repository.Sysinfo');
	}

	async requestTroubleShooting() {
		const { data } = await tryExecuteAndNotify(this, ServerService.getServerTroubleshooting());
		return data;
	}

	async requestServerInformation() {
		const { data } = await tryExecuteAndNotify(this, ServerService.getServerInformation());
		return data;
	}

	async requestServerConfiguration() {
		const { data } = await tryExecute(ServerService.getServerConfiguration());
		return data;
	}

	async serverUpgradeCheck(): Promise<UmbServerUpgradeCheck | null> {
		// Check if we are allowed to check again
		const versionCheckPeriod = await this.#getVersionCheckPeriod();

		if (versionCheckPeriod <= 0) {
			return null;
		}

		let shouldCheck = true;

		const lastCheck = localStorage.getItem('umb:lastUpgradeCheck');
		const now = new Date();
		if (lastCheck) {
			const lastCheckDate = new Date(lastCheck);
			const diff = now.getTime() - lastCheckDate.getTime();
			const diffDays = diff / (1000 * 3600 * 24);

			if (diffDays < versionCheckPeriod) {
				shouldCheck = false;
			}

			// If we should not check, then return what we have stored if it is still valid
			if (!shouldCheck) {
				return this.#getStoredServerUpgradeCheck(lastCheckDate);
			}
		}

		if (!shouldCheck) {
			return null;
		}

		// Check the server
		const { data } = await tryExecute(ServerService.getServerUpgradeCheck());

		if (data) {
			// Save the last check date including the data received
			const expiresAt = new Date();
			expiresAt.setDate(expiresAt.getDate() + versionCheckPeriod);
			const upgradeCheck = { ...data, expires: expiresAt.toISOString() } satisfies UmbServerUpgradeCheck;
			localStorage.setItem('umb:serverUpgradeCheck', JSON.stringify(upgradeCheck));
			localStorage.setItem('umb:lastUpgradeCheck', now.toISOString());

			// Only return if we have a valid type
			if (data.type.toLowerCase() !== 'none') {
				return upgradeCheck;
			}
		}

		return null;
	}

	async #getVersionCheckPeriod(): Promise<number> {
		if (!this.#serverConfiguration) {
			this.#serverConfiguration = await this.requestServerConfiguration();
		}

		return this.#serverConfiguration?.versionCheckPeriod ?? 7;
	}

	#getStoredServerUpgradeCheck(lastCheck: Date): UmbServerUpgradeCheck | null {
		const storedCheck = localStorage.getItem('umb:serverUpgradeCheck');
		if (storedCheck) {
			const upgradeCheck: UmbServerUpgradeCheck = JSON.parse(storedCheck);
			// Check that the stored check is not older than the last check
			const expiresAt = new Date(upgradeCheck.expires);
			if (expiresAt.getTime() > lastCheck.getTime()) {
				if (upgradeCheck.type.toLowerCase() !== 'none') {
					return upgradeCheck;
				}
			} else {
				localStorage.removeItem('umb:serverUpgradeCheck');
			}
		}

		return null;
	}
}
