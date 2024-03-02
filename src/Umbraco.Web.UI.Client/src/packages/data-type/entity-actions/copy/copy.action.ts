import type { UmbCopyDataTypeRepository } from '../../repository/copy/data-type-copy.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_DATA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

// TODO: investigate what we need to make a generic copy action
export class UmbCopyDataTypeEntityAction extends UmbEntityActionBase<UmbCopyDataTypeRepository> {
	async execute() {
		if (!this.repository) throw new Error('Repository is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DATA_TYPE_PICKER_MODAL);
		const value = await modalContext.onSubmit();
		if (!value) return;
		await this.repository.copy(this.unique, value.selection[0]);
	}
}
