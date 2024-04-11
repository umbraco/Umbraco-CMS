import { UmbDocumentCreateBlueprintRepository } from './repository/document-create-blueprint.repository.js';
import { UMB_CREATE_BLUEPRINT_MODAL } from './modal/create-blueprint-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDocumentBlueprintEntityAction extends UmbEntityActionBase<never> {
	#repository = new UmbDocumentCreateBlueprintRepository(this);

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Unique is required');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_CREATE_BLUEPRINT_MODAL, {
			data: { unique: this.args.unique },
		});
		await modalContext.onSubmit().catch(() => undefined);

		const { name, parent } = modalContext.getValue();
		if (!name) return;

		// TODO: Doesn't show the green popup on success? tryExecuteAndNotify is used in the repository.
		await this.#repository.create({ name, parent, document: { id: this.args.unique } });
	}
}
export default UmbCreateDocumentBlueprintEntityAction;
