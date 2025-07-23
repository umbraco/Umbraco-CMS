import { UmbExportDocumentTypeRepository } from './repository/index.js';
import { blobDownload } from '@umbraco-cms/backoffice/utils';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbExportDocumentTypeEntityAction extends UmbEntityActionBase<object> {
	#repository = new UmbExportDocumentTypeRepository(this);

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const { data, error } = await this.#repository.requestExport(this.args.unique);
		if (error) {
			throw error;
		}

		blobDownload(data, `${this.args.unique}.udt`, 'text/xml');
	}
}

export default UmbExportDocumentTypeEntityAction;
