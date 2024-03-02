import { UMB_PUBLIC_ACCESS_MODAL } from './modal/public-access-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<UmbDocumentDetailRepository> {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_PUBLIC_ACCESS_MODAL, { data: { unique: this.unique } });
	}
}
