import { UmbExportDocumentTypeServerDataSource } from './document-type-export.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbExportDocumentTypeRepository extends UmbRepositoryBase {
	#exportSource = new UmbExportDocumentTypeServerDataSource(this);

	async requestExport(unique: string) {
		return this.#exportSource.export(unique);
	}
}

export { UmbExportDocumentTypeRepository as api };
