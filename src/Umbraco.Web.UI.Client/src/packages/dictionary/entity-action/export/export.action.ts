import { UmbDictionaryExportRepository } from '../../repository/index.js';
import { UMB_EXPORT_DICTIONARY_MODAL } from './export-dictionary-modal.token.js';
import { blobDownload } from '@umbraco-cms/backoffice/utils';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbExportDictionaryEntityAction extends UmbEntityActionBase<object> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');

		const value = await umbOpenModal(this, UMB_EXPORT_DICTIONARY_MODAL, { data: { unique: this.args.unique } });

		// Export the file
		const repository = new UmbDictionaryExportRepository(this);
		const { data } = await repository.requestExport(this.args.unique, value.includeChildren);

		if (!data) return;

		blobDownload(data, `${this.args.unique}.udt`, 'text/xml');
	}
}

export default UmbExportDictionaryEntityAction;
