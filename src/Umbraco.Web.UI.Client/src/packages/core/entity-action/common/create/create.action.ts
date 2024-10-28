import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { MetaEntityActionCreateKind } from './types.js';
import { UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL } from './modal/index.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateEntityAction extends UmbEntityActionBase<MetaEntityActionCreateKind> {
	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
			},
		});

		await modalContext.onSubmit();
	}
}

export { UmbCreateEntityAction as api };
