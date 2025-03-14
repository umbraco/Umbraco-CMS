import { UmbDocumentCreateBlueprintRepository } from './repository/document-create-blueprint.repository.js';
import { UMB_CREATE_BLUEPRINT_MODAL } from './constants.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbCreateDocumentBlueprintEntityAction extends UmbEntityActionBase<never> {
	#repository = new UmbDocumentCreateBlueprintRepository(this);

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is required');

		const value = await umbOpenModal(this, UMB_CREATE_BLUEPRINT_MODAL, {
			data: { unique: this.args.unique },
		});

		const { name, parent } = value;
		if (!name) return;

		await this.#repository.create({ name, parent, document: { id: this.args.unique } });
	}
}
export default UmbCreateDocumentBlueprintEntityAction;
