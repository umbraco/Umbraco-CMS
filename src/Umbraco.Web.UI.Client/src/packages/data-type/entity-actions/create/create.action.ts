import type { UmbDataTypeDetailRepository } from '../../repository/detail/data-type-detail.repository.js';
import { UMB_DATA_TYPE_CREATE_OPTIONS_MODAL } from './modal/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDataTypeEntityAction extends UmbEntityActionBase<UmbDataTypeDetailRepository> {
	async execute() {
		if (!this.repository) throw new Error('Repository is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_DATA_TYPE_CREATE_OPTIONS_MODAL, {
			data: {
				parentUnique: this.unique,
			},
		});
	}
}
