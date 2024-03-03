import { UMB_DATA_TYPE_CREATE_OPTIONS_MODAL } from './modal/index.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDataTypeEntityAction extends UmbEntityActionBase<UmbEntityActionArgs<never>> {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_DATA_TYPE_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					entityType: this.args.entityType,
					unique: this.args.unique,
				},
			},
		});

		await modalContext.onSubmit();
	}

	destroy(): void {}
}
