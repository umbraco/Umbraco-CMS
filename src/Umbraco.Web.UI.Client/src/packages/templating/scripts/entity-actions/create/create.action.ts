import { UMB_SCRIPT_CREATE_OPTIONS_MODAL } from './options-modal/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbScriptCreateOptionsEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_SCRIPT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					entityType: this.args.entityType,
					unique: this.args.unique,
				},
			},
		});

		await modalContext.onSubmit();
	}
}

export { UmbScriptCreateOptionsEntityAction as api };
