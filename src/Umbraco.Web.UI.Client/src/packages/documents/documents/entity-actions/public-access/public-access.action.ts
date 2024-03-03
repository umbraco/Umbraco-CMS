import { UMB_PUBLIC_ACCESS_MODAL } from './modal/public-access-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbDocumentDetailRepository, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_PUBLIC_ACCESS_MODAL, { data: { unique: this.args.unique } });
	}

	destroy(): void {}
}
