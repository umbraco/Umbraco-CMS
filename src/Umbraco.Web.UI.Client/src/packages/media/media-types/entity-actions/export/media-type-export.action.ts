import { UmbExportMediaTypeRepository } from './repository/index.js';
import { blobDownload } from '@umbraco-cms/backoffice/utils';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbExportMediaTypeEntityAction extends UmbEntityActionBase<object> {
	#repository = new UmbExportMediaTypeRepository(this);

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const { data } = await this.#repository.requestExport(this.args.unique);
		if (!data) return;

		blobDownload(data, `${this.args.unique}.udt`, 'text/xml');
	}
}

export default UmbExportMediaTypeEntityAction;
