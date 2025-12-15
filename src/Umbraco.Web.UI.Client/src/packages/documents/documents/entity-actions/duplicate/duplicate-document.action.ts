import type { UmbDocumentItemModel } from '../../item/repository/types.js';
import { UmbDocumentItemRepository } from '../../item/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DUPLICATE_DOCUMENT_MODAL } from './modal/index.js';
import { UmbDuplicateDocumentRepository } from './repository/index.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbDocumentTypeStructureRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbDuplicateDocumentEntityAction extends UmbEntityActionBase<never> {
	#localize = new UmbLocalizationController(this);
	#duplicateRepository = new UmbDuplicateDocumentRepository(this);
	#itemRepository = new UmbDocumentItemRepository(this);
	#structureRepository = new UmbDocumentTypeStructureRepository(this);
	#sourceItem?: UmbDocumentItemModel;
	#destinationUnique?: string | null;

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		// Fetch source item to get its document type
		const { data } = await this.#itemRepository.requestItems([this.args.unique]);
		if (!data?.length) throw new Error('Source item not found');
		this.#sourceItem = data[0];

		await umbOpenModal(this, UMB_DUPLICATE_DOCUMENT_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				name: this.#sourceItem.variants[0]?.name,
				onSelection: async (destinationUnique: string | null) => this.#onSelection(destinationUnique),
				onBeforeSubmit: async (
					destinationUnique: string | null,
					options: { relateToOriginal: boolean; includeDescendants: boolean },
				) => this.#onBeforeSubmit(destinationUnique, options),
			},
		});

		this.#reloadMenu(this.#destinationUnique ?? null);
	}

	async #onSelection(destinationUnique: string | null): Promise<{ valid: boolean; error?: string }> {
		if (!this.#sourceItem) {
			return { valid: false, error: this.#localize.term('general_error') };
		}

		// Root is always valid (will be validated by onBeforeSubmit if not allowed)
		if (destinationUnique === null) {
			return { valid: true };
		}

		// Fetch destination item to get its document type
		const { data: destinationItems } = await this.#itemRepository.requestItems([destinationUnique]);
		if (!destinationItems?.length) {
			return { valid: false, error: this.#localize.term('general_error') };
		}
		const destinationItem = destinationItems[0];

		// Get allowed children of the destination's document type
		const { data: allowedChildren } = await this.#structureRepository.requestAllowedChildrenOf(
			destinationItem.documentType.unique,
			destinationUnique,
		);

		if (!allowedChildren?.items) {
			return { valid: false, error: this.#localize.term('general_error') };
		}

		// Check if source document type is allowed
		const isAllowed = allowedChildren.items.some((allowed) => allowed.unique === this.#sourceItem!.documentType.unique);

		if (!isAllowed) {
			return {
				valid: false,
				error: this.#localize.term('moveOrCopy_notAllowedByContentType'),
			};
		}

		return { valid: true };
	}

	async #onBeforeSubmit(
		destinationUnique: string | null,
		options: { relateToOriginal: boolean; includeDescendants: boolean },
	): Promise<{ success: boolean; error?: { message: string } }> {
		if (!this.args.unique) {
			return { success: false, error: { message: 'Unique is not available' } };
		}

		const { error } = await this.#duplicateRepository.requestDuplicate({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
			relateToOriginal: options.relateToOriginal,
			includeDescendants: options.includeDescendants,
		});

		if (error) {
			return { success: false, error: { message: error.message } };
		}

		this.#destinationUnique = destinationUnique;
		return { success: true };
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
