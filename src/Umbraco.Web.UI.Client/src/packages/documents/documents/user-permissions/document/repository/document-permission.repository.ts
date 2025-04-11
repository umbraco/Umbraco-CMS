import { UmbDocumentPermissionServerDataSource } from './document-permission.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentPermissionRepository extends UmbControllerBase {
	#permissionSource: UmbDocumentPermissionServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#permissionSource = new UmbDocumentPermissionServerDataSource(this);
	}

	async requestPermissions(id: string) {
		if (!id) throw new Error(`id is required`);
		return this.#permissionSource.requestPermissions(id);
	}
}

export default UmbDocumentPermissionRepository;
