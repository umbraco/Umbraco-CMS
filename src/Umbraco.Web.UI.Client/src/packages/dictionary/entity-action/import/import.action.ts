import { UMB_IMPORT_DICTIONARY_MODAL } from './import-dictionary-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbImportDictionaryEntityAction extends UmbEntityActionBase<object> {
	override async execute() {
		await umbOpenModal(this, UMB_IMPORT_DICTIONARY_MODAL, { data: { unique: this.args.unique } });
	}
}

export default UmbImportDictionaryEntityAction;
