import { UMB_DOCUMENT_TYPE_IMPORT_MODAL } from './modal/document-type-import-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbImportDocumentTypeEntityAction extends UmbEntityActionBase<object> {
	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DOCUMENT_TYPE_IMPORT_MODAL, {
			data: { unique: this.args.unique },
		});
		await modalContext.onSubmit().catch(() => {});
	}
}

export default UmbImportDocumentTypeEntityAction;
