import packageJson from '../../../../package.json';
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
}
