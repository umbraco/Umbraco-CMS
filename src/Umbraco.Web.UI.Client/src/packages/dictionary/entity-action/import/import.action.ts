import { UMB_IMPORT_DICTIONARY_MODAL } from './import-dictionary-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbImportDictionaryEntityAction extends UmbEntityActionBase<object> {
	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_IMPORT_DICTIONARY_MODAL, { data: { unique: this.args.unique } });
		await modalContext.onSubmit();
	}
}

export default UmbImportDictionaryEntityAction;
