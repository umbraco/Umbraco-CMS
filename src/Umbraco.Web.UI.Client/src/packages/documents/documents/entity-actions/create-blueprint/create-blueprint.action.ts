import { UMB_CREATE_BLUEPRINT_MODAL } from './modal/create-blueprint-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbDocumentBlueprintDetailRepository } from '@umbraco-cms/backoffice/document-blueprint';

export class UmbCreateDocumentBlueprintEntityAction extends UmbEntityActionBase<never> {
	#repository = new UmbDocumentBlueprintDetailRepository(this);

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_CREATE_BLUEPRINT_MODAL, {
			data: { unique: this.args.unique },
		});
		await modalContext.onSubmit().catch(() => undefined);
	}
}
export default UmbCreateDocumentBlueprintEntityAction;
