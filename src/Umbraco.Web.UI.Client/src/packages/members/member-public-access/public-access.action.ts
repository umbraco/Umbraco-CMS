import { UMB_PUBLIC_ACCESS_MODAL } from './modal/public-access-modal.token.js';
import {
	UmbEntityActionBase,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		await umbOpenModal(this, UMB_PUBLIC_ACCESS_MODAL, { data: { unique: this.args.unique } });
		this.#requestReloadEntity();
	}

	async #requestReloadEntity() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context is not available');
		}

		const entityStructureEvent = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		const entityChildrenEvent = new UmbRequestReloadChildrenOfEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(entityStructureEvent);
		actionEventContext.dispatchEvent(entityChildrenEvent);
	}
}

export { UmbDocumentPublicAccessEntityAction as api };
