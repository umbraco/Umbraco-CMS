import { t } from 'msw/lib/glossary-de6278a9.js';
import { UmbDocumentPermissionServerDataSource } from './document-permission.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPermissionRepository {
	#host: UmbControllerHostElement;

	#permissionSource: UmbDocumentPermissionServerDataSource;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		this.#permissionSource = new UmbDocumentPermissionServerDataSource(this.#host);
	}

	async requestPermissions(id: string) {
		if (!id) throw new Error(`id is required`);
		return this.#permissionSource.requestPermissions(id);
	}
}
