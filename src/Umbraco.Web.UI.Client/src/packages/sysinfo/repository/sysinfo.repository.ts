import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';

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
}
