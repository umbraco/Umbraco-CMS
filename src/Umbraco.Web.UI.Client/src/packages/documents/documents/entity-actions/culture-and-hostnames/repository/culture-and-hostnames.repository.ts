import { UmbDocumentCultureAndHostnamesServerDataSource } from './culture-and-hostnames.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UpdateDomainsRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentCultureAndHostnamesRepository extends UmbControllerBase implements UmbApi {
	#dataSource = new UmbDocumentCultureAndHostnamesServerDataSource(this);

	async readCultureAndHostnames(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#dataSource.read(unique);
	}

	async updateCultureAndHostnames(unique: string, data: UpdateDomainsRequestModel) {
		if (!unique) throw new Error('Unique is missing');
		if (!data) throw new Error('Data is missing');
		return this.#dataSource.update(unique, data);
	}
}

export { UmbDocumentCultureAndHostnamesRepository as api };
