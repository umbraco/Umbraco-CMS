import type { UmbDocumentItemModel } from '../../item/repository/types.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UmbDocumentItemRepository } from '../../item/index.js';
import { UMB_DOCUMENT_TREE_ALIAS } from '../../tree/index.js';
import { UMB_BULK_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './repository/constants.js';
import type { UmbBulkMoveToRepository } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';
import { UMB_MOVE_TO_MODAL } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbDocumentTypeStructureRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbMoveDocumentBulkAction extends UmbEntityBulkActionBase<never> {
	#localize = new UmbLocalizationController(this);
	#itemRepository = new UmbDocumentItemRepository(this);
	#structureRepository = new UmbDocumentTypeStructureRepository(this);
	#sourceItems: UmbDocumentItemModel[] = [];
	#disallowedDocumentTypes = new Set<string>();

	async execute() {
		if (this.selection?.length === 0) return;

		// Fetch all selected items to get their document types
		const { data } = await this.#itemRepository.requestItems(this.selection);
		if (!data?.length) throw new Error('Source items not found');
		this.#sourceItems = data;

		const bulkMoveRepository = await createExtensionApiByAlias<UmbBulkMoveToRepository>(
			this,
			UMB_BULK_MOVE_DOCUMENT_REPOSITORY_ALIAS,
		);
		if (!bulkMoveRepository) throw new Error('Bulk Move Repository is not available');

		await umbOpenModal(this, UMB_MOVE_TO_MODAL, {
			data: {
				unique: this.selection[0], // Use first item for entity context
				entityType: 'document',
				treeAlias: UMB_DOCUMENT_TREE_ALIAS,
				name: `${this.selection.length} ${this.#localize.term('defaultdialogs_items')}`,
				pickableFilter: (treeItem: UmbTreeItemModel) => {
					// Prevent selecting any of the source items
					if (this.selection.includes(treeItem.unique as string)) return false;

					const docTreeItem = treeItem as UmbDocumentTreeItemModel;

					// Prevent moving to a descendant of any selected item (would create circular reference)
					const isDescendantOfAny = this.selection.some((sourceUnique) =>
						docTreeItem.ancestors?.some((ancestor) => ancestor.unique === sourceUnique),
					);
					if (isDescendantOfAny) return false;

					// Prevent selecting document types that have already been found to be disallowed
					const documentType = docTreeItem.documentType?.unique;
					if (documentType && this.#disallowedDocumentTypes.has(documentType)) return false;

					return true;
				},
				onSelection: async (destinationUnique: string | null) => this.#onSelection(destinationUnique),
				onBeforeSubmit: async (destinationUnique: string | null) =>
					this.#onBeforeSubmit(bulkMoveRepository, destinationUnique),
			},
		});

		await this.#reloadMenu();
	}

	async #onSelection(destinationUnique: string | null): Promise<{ valid: boolean; error?: string }> {
		if (!this.#sourceItems.length) {
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

		const allowedTypeUniques = new Set(allowedChildren.items.map((item) => item.unique));

		// Check which items are NOT allowed
		const invalidItems = this.#sourceItems.filter((item) => !allowedTypeUniques.has(item.documentType.unique));

		if (invalidItems.length > 0) {
			// Add to disallowed types so all items of this type become unselectable
			this.#disallowedDocumentTypes.add(destinationItem.documentType.unique);

			// Get names of invalid items (use variant name or fallback)
			const invalidNames = invalidItems.map((item) => item.variants?.[0]?.name || item.unique);

			if (invalidItems.length === this.#sourceItems.length) {
				// All items are invalid
				return {
					valid: false,
					error: this.#localize.term('moveOrCopy_notAllowedByContentType'),
				};
			}

			// Some items are invalid - list them
			return {
				valid: false,
				error: `${this.#localize.term('moveOrCopy_notAllowedByContentType')}: ${invalidNames.join(', ')}`,
			};
		}

		return { valid: true };
	}

	async #onBeforeSubmit(
		bulkMoveRepository: UmbBulkMoveToRepository,
		destinationUnique: string | null,
	): Promise<{ success: boolean; error?: { message: string } }> {
		const { error } = await bulkMoveRepository.requestBulkMoveTo({
			uniques: this.selection,
			destination: { unique: destinationUnique },
		});

		if (error) {
			return { success: false, error: { message: error.message } };
		}

		return { success: true };
	}

	async #reloadMenu() {
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) throw new Error('Entity Context is not available');

		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (entityType && unique !== undefined) {
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			if (!eventContext) throw new Error('Event Context is not available');

			const args = { entityType, unique };

			const reloadChildren = new UmbRequestReloadChildrenOfEntityEvent(args);
			eventContext.dispatchEvent(reloadChildren);

			const reloadStructure = new UmbRequestReloadStructureForEntityEvent(args);
			eventContext.dispatchEvent(reloadStructure);
		}
	}
}

export { UmbMoveDocumentBulkAction as api };
