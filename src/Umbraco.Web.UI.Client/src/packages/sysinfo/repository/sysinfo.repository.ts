import type { UmbServerUpgradeCheck } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute, tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

export class UmbSysinfoRepository extends UmbRepositoryBase {
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

	/**
	 * Check if the server has an upgrade available and return the result.
	 * If the server has an upgrade available, the result will be stored in local storage.
	 * If the server does not have an upgrade available, the result will be stored in local storage as well.
	 * @param {string} currentVersion The current version of the server.
	 * @returns {Promise<UmbServerUpgradeCheck | null>} The server upgrade check result or null if the check is not allowed or if the check failed.
	 */
	async serverUpgradeCheck(currentVersion: string): Promise<UmbServerUpgradeCheck | null> {
		// Check if we are allowed to check again
		const appContext = await this.getContext(UMB_APP_CONTEXT);
		const versionCheckPeriod = await this.observe(appContext.getServerConnection().versionCheckPeriod).asPromise();

		if (versionCheckPeriod <= 0) {
			// We do not need to check the server for an upgrade
			return null;
		}

		const lastUpgradeCheck = this.#getStoredServerUpgradeCheck(currentVersion);

		// If we have a stored check, then return it if it is still valid
		if (lastUpgradeCheck !== null) {
			// If we have a stored check, then check if we should check again based on the period
			if (lastUpgradeCheck.createdAt) {
				const lastCheckDate = new Date(lastUpgradeCheck.createdAt);
				const diff = new Date().getTime() - lastCheckDate.getTime();
				const diffDays = diff / (1000 * 3600 * 24);

				if (diffDays < versionCheckPeriod) {
					// If we should not check, then return what we have stored if it is still valid
					if (lastUpgradeCheck.type.toLowerCase() !== 'none') {
						return lastUpgradeCheck;
					}
					return null; // no upgrade available
				}
			}
		}

		// Check the server for an upgrade because we have no stored check or the stored check is invalid
		return this.#fetchServerUpgradeCheck(versionCheckPeriod, currentVersion);
	}

	/**
	 * Get the stored server upgrade check if it is still valid, otherwise return null and remove the stored check.
	 * @param {string} currentVersion The current version of the server.
	 * @returns {UmbServerUpgradeCheck | null} The stored server upgrade check or null if it is not valid.
	 */
	#getStoredServerUpgradeCheck(currentVersion: string): UmbServerUpgradeCheck | null {
		const storedCheck = localStorage.getItem('umb:serverUpgradeCheck');
		if (!storedCheck) {
			return null;
		}

		const upgradeCheck: UmbServerUpgradeCheck = JSON.parse(storedCheck);

		// Check that the stored check is for the same version
		if (upgradeCheck.version !== currentVersion) {
			localStorage.removeItem('umb:serverUpgradeCheck');
			return null;
		}

		// Check that the stored check is not older than the last check
		if (upgradeCheck.createdAt) {
			const createdAt = new Date(upgradeCheck.createdAt);
			const expiresAt = new Date(upgradeCheck.expires);
			if (expiresAt.getTime() <= createdAt.getTime()) {
				localStorage.removeItem('umb:serverUpgradeCheck');
				return null;
			}
		}

		return upgradeCheck;
	}

	/**
	 * Fetch the server upgrade check from the server and store the result in local storage.
	 * @param {number} versionCheckPeriod A period in days to wait before checking the server again.
	 * @param {string} currentVersion The current version of the server.
	 * @returns {Promise<UmbServerUpgradeCheck | null>} The server upgrade check result or null if the check failed.
	 */
	async #fetchServerUpgradeCheck(
		versionCheckPeriod: number,
		currentVersion: string,
	): Promise<UmbServerUpgradeCheck | null> {
		// Check the server for an upgrade because we have no stored check or the stored check is invalid
		const { data } = await tryExecute(ServerService.getServerUpgradeCheck());

		if (data) {
			// Save the last check date including the data received
			const expiresAt = new Date();
			expiresAt.setDate(expiresAt.getDate() + versionCheckPeriod);
			const upgradeCheck = {
				...data,
				expires: expiresAt.toISOString(),
				version: currentVersion,
				createdAt: new Date().toISOString(),
			} satisfies UmbServerUpgradeCheck;
			localStorage.setItem('umb:serverUpgradeCheck', JSON.stringify(upgradeCheck));

			// Only return if we have a valid type
			if (data.type.toLowerCase() !== 'none') {
				return upgradeCheck;
			}
		}

		return null; // no upgrade available
	}
}
