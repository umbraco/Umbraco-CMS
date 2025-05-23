import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DUPLICATE_DOCUMENT_MODAL } from './modal/index.js';
import { UmbDuplicateDocumentRepository } from './repository/index.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';

export class UmbDuplicateDocumentEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const value = await umbOpenModal(this, UMB_DUPLICATE_DOCUMENT_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
			},
		});

		const destinationUnique = value.destination.unique;
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const duplicateRepository = new UmbDuplicateDocumentRepository(this);

		const { error } = await duplicateRepository.requestDuplicate({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
			relateToOriginal: value.relateToOriginal,
			includeDescendants: value.includeDescendants,
		});

		if (error) {
			throw error;
		}

		this.#reloadMenu(destinationUnique);
	}

	async #reloadMenu(destinationUnique: string | null) {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context is not available');
		}

		// When duplicating, the destination entity type may or may not be the same as that of
		// the item selected for duplication (that is available in this.args).
		// For documents though, we know the entity type will be "document", unless we are duplicating
		// to the root (when the destinationUnique will be null).
		const destinationEntityType = destinationUnique === null ? UMB_DOCUMENT_ROOT_ENTITY_TYPE : UMB_DOCUMENT_ENTITY_TYPE;

		const event = new UmbRequestReloadChildrenOfEntityEvent({
			unique: destinationUnique,
			entityType: destinationEntityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export { UmbDuplicateDocumentEntityAction as api };
