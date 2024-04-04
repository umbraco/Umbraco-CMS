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

	async requestVersionsByDocumentId(id: string, culture: string) {
		const { data, error } = await this.#dataSource.getVersionsByDocumentId(id, culture);
		return { data, error };
	}

	async requestVersionById(id: string) {
		const { data, error } = await this.#dataSource.getVersionById(id);
		return { data, error };
	}

	async setPreventCleanup(versionId: string, preventCleanup: boolean) {
		const { error } = await this.#dataSource.setPreventCleanup(versionId, preventCleanup);
		return { error };
	}

	async rollback(versionId: string, culture?: string) {
		const { error } = await this.#dataSource.rollback(versionId, culture);
		return { error };
	}
}

export default UmbRollbackRepository;
