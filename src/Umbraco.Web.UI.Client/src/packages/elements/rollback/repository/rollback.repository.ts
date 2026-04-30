import { UmbElementRollbackServerDataSource } from './rollback.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbElementRollbackRepository extends UmbControllerBase implements UmbApi {
	#dataSource: UmbElementRollbackServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbElementRollbackServerDataSource(this);
	}

	async requestVersionsByElementId(id: string, culture?: string) {
		return await this.#dataSource.getVersionsByElementId(id, culture);
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

export default UmbElementRollbackRepository;
