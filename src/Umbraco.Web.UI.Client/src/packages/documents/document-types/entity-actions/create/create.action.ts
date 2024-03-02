import type { UmbDocumentTypeDetailRepository } from '../../repository/detail/document-type-detail.repository.js';
import { UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL } from './modal/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDataTypeEntityAction extends UmbEntityActionBase<UmbDocumentTypeDetailRepository> {
	async execute() {
		if (!this.repository) throw new Error('Repository is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					unique: this.unique,
					entityType: this.entityType,
				},
			},
		});
	}
}
