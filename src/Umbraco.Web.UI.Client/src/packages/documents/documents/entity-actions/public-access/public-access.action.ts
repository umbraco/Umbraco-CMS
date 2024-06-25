import { UMB_PUBLIC_ACCESS_MODAL } from './modal/public-access-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import {
	UmbEntityActionBase,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbDocumentDetailRepository, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_PUBLIC_ACCESS_MODAL, { data: { unique: this.args.unique } });
		await modal.onSubmit();
		this.#requestReloadEntity();
	}

	async #requestReloadEntity() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

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
