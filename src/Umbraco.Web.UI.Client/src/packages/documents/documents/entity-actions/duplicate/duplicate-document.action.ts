import { UMB_DUPLICATE_DOCUMENT_MODAL } from './modal/index.js';
import { UmbDuplicateDocumentRepository } from './repository/index.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';

export class UmbDuplicateDocumentEntityAction extends UmbEntityActionBase<any> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_DUPLICATE_DOCUMENT_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
			},
		});

		try {
			const value = await modal.onSubmit();
			const destinationUnique = value.destination.unique;
			if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

			const duplicateRepository = new UmbDuplicateDocumentRepository(this);

			const { error } = await duplicateRepository.requestDuplicate({
				unique: this.args.unique,
				destination: { unique: destinationUnique },
				relateToOriginal: value.relateToOriginal,
				includeDescendants: value.includeDescendants,
			});

			if (!error) {
				this.#reloadMenu();
			}
		} catch (error) {
			console.log(error);
		}
	}

	async #reloadMenu() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
		// TODO: Reload destination
	}
}

export { UmbDuplicateDocumentEntityAction as api };
