import type { UmbMoveDataTypeRepository } from '../../repository/move/data-type-move.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_DATA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

// TODO: investigate what we need to make a generic move action
export class UmbMoveDataTypeEntityAction extends UmbEntityActionBase<UmbMoveDataTypeRepository> {
	async execute() {
		if (!this.unique) throw new Error('Unique is not available');
		if (!this.repository) throw new Error('Repository is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DATA_TYPE_PICKER_MODAL);
		const { selection } = await modalContext.onSubmit();
		await this.repository.move(this.unique, selection[0]);
	}
}
