import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentItemRepository } from '../../item/index.js';
import { UMB_DUPLICATE_DOCUMENT_MODAL } from './modal/index.js';
import { UmbDuplicateDocumentRepository } from './repository/index.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UmbDocumentTypeDetailRepository,
	UmbDocumentTypeStructureRepository,
} from '@umbraco-cms/backoffice/document-type';

export class UmbDuplicateDocumentEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const duplicateRepository = new UmbDuplicateDocumentRepository(this);
		const selectableFilter = await this.#getSelectableFilterByDocumentUnique(this.args.unique);

		const value = await umbOpenModal(this, UMB_DUPLICATE_DOCUMENT_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				selectableFilter,
			},
		});

		const destinationUnique = value.destination.unique;
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

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

	async #getSelectableFilterByDocumentUnique(documentUnique: string) {
		// 1. Get the document to find its type
		const itemRepository = new UmbDocumentItemRepository(this);
		const { data } = await itemRepository.requestItems([documentUnique]);
		const item = data?.[0];

		if (!item) throw new Error('Item is not available');

		const documentTypeUnique = item.documentType.unique;

		const structureRepository = new UmbDocumentTypeStructureRepository(this);
		const typeDetailRepository = new UmbDocumentTypeDetailRepository(this);

		const [{ data: allowedParents }, { data: documentType }] = await Promise.all([
			structureRepository.requestAllowedParentsOf(documentTypeUnique),
			typeDetailRepository.requestByUnique(documentTypeUnique),
		]);
		const isAllowedAtRoot = documentType?.allowedAtRoot ?? false;

		if (allowedParents) {
			return (treeItem: any) => {
				if (treeItem.unique === null) {
					return isAllowedAtRoot;
				}
				return allowedParents.some((parent) => parent.unique === treeItem.documentType?.unique);
			};
		}

		return undefined;
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
