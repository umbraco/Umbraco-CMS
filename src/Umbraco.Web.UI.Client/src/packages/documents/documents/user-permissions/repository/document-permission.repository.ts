import { UmbDocumentPermissionServerDataSource } from './document-permission.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentPermissionRepository extends UmbBaseController {
	#permissionSource: UmbDocumentPermissionServerDataSource;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.#permissionSource = new UmbDocumentPermissionServerDataSource(this);
	}

	async requestPermissions(id: string) {
		if (!id) throw new Error(`id is required`);
		return this.#permissionSource.requestPermissions(id);
	}
}
