import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_CULTURE_AND_HOSTNAMES_MODAL, type UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';

export class UmbDocumentCultureAndHostnamesEntityAction extends UmbEntityActionBase<UmbDocumentDetailRepository> {
	async execute() {
		if (!this.repository) return;
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_CULTURE_AND_HOSTNAMES_MODAL, {
			data: { unique: this.unique ?? null },
		});
	}
}
