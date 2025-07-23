import { UMB_DOCUMENT_TYPE_IMPORT_MODAL } from './modal/document-type-import-modal.token.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbImportDocumentTypeEntityAction extends UmbEntityActionBase<object> {
	override async execute() {
		await umbOpenModal(this, UMB_DOCUMENT_TYPE_IMPORT_MODAL, {
			data: { unique: this.args.unique },
		});

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context is not available');
		}
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export default UmbImportDocumentTypeEntityAction;
