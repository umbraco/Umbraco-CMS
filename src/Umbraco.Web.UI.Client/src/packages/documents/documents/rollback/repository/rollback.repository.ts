import { UmbRollbackServerDataSource } from './rollback.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbRollbackRepository extends UmbControllerBase implements UmbApi {
	#dataSource: UmbRollbackServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbRollbackServerDataSource(this);
	}

	async requestVersionsByDocumentId(id: string, culture?: string) {
		return await this.#dataSource.getVersionsByDocumentId(id, culture);
	}

	async requestVersionById(id: string) {
		return await this.#dataSource.getVersionById(id);
	}

	async setPreventCleanup(versionId: string, preventCleanup: boolean) {
		return await this.#dataSource.setPreventCleanup(versionId, preventCleanup);
	}

	async rollback(versionId: string, culture?: string) {
		return await this.#dataSource.rollback(versionId, culture);
	}
}

export default UmbRollbackRepository;
