import packageJson from '../../../../package.json';
import type { UmbServerUpgradeCheck } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbSysinfoRepository extends UmbRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Repository.Sysinfo');
	}

	async requestTroubleShooting() {
		const { data } = await tryExecute(this, ServerService.getServerTroubleshooting(), { disableNotifications: true });
		return data;
	}

	async requestServerInformation() {
		const { data } = await tryExecute(this, ServerService.getServerInformation(), { disableNotifications: true });
		return data;
	}

	async requestClientInformation() {
		const { version } = packageJson;
		const clientInformation = {
			version,
		};
		return clientInformation;
	}

	/**
	 * This method used to check for server upgrades by comparing the current version with the latest available version.
	 * However, the upgrade check functionality is now defunct and this method will always return null.
	 * @param {string} _currentVersion The current version of the server.
	 * @returns {Promise<UmbServerUpgradeCheck | null>} The server upgrade check result or null if the check is not allowed or if the check failed.
	 * @deprecated This method is deprecated and will be removed in Umbraco 19. It is no longer possible to check for updates from the backoffice.
	 */

	async serverUpgradeCheck(_currentVersion: string): Promise<UmbServerUpgradeCheck | null> {
		return null;
	}
}
