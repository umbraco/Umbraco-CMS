import { UMB_MEDIA_TREE_PICKER_MODAL } from '../../tree/index.js';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		// TODO: the picker should be single picker by default
		const modalContext = modalManager.open(this, UMB_MEDIA_TREE_PICKER_MODAL, {
			data: {
				multiple: false,
			},
			value: {
				selection: [],
			},
		});
		if (modalContext) {
			//const { selection } = await modalContext.onSubmit();
			//const destination = selection[0];
			//await this.repository?.move(this.selection, destination);
		}
	}
}

export { UmbMediaMoveEntityBulkAction as api };
