import { UMB_MEDIA_TYPE_IMPORT_MODAL } from './modal/media-type-import-modal.token.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT, umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbImportMediaTypeEntityAction extends UmbEntityActionBase<object> {
	override async execute() {
		await umbOpenModal(this, UMB_MEDIA_TYPE_IMPORT_MODAL, {
			data: { unique: this.args.unique },
		}).catch(() => {});

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context not found.');
		}
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export default UmbImportMediaTypeEntityAction;
