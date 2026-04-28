import type { UmbDocumentRedirectFilterArgs } from './types.js';
import { UmbDocumentRedirectManagementServerDataSource } from './document-redirect-management.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentRedirectManagementRepository extends UmbControllerBase implements UmbApi {
	#dataSource = new UmbDocumentRedirectManagementServerDataSource(this);

	async requestStatus() {
		return this.#dataSource.getStatus();
	}

	async setStatus(enabled: boolean) {
		return this.#dataSource.setStatus(enabled);
	}

	async requestByDocumentUnique(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#dataSource.getByDocumentUnique(unique);
	}

	async requestRedirects(args: UmbDocumentRedirectFilterArgs = {}) {
		return this.#dataSource.filter(args);
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#dataSource.delete(unique);
	}
}

export { UmbDocumentRedirectManagementRepository as api };
