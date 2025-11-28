import packageJson from '../../../../package.json';
import type { UmbServerUpgradeCheck } from '../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

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
	 * Check if the server has an upgrade available and return the result.
	 * If the server has an upgrade available, the result will be stored in local storage.
	 * If the server does not have an upgrade available, the result will be stored in local storage as well.
	 * @param {string} _currentVersion The current version of the server.
	 * @returns {Promise<UmbServerUpgradeCheck | null>} The server upgrade check result or null if the check is not allowed or if the check failed.
	 * @deprecated This method is deprecated and will be removed in Umbraco 19. It is no longer possible to check for updates from the backoffice.
	 */
	async serverUpgradeCheck(_currentVersion: string): Promise<UmbServerUpgradeCheck | null> {
		return Promise.resolve({
			comment: '',
			type: '',
			url: '',
			expires: '',
		} satisfies UmbServerUpgradeCheck);
	}
}
