import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { UmbMediaItemRepository } from '../../repository/index.js';
import { UMB_MEDIA_TREE_ALIAS } from '../../constants.js';
import { UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
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
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbMoveMediaBulkAction extends UmbEntityBulkActionBase<never> {
	#localize = new UmbLocalizationController(this);
	#itemRepository = new UmbMediaItemRepository(this);
	#structureRepository = new UmbMediaTypeStructureRepository(this);
	#sourceItems: UmbMediaItemModel[] = [];

	async execute() {
		if (this.selection?.length === 0) return;

		// Fetch all selected items to get their media types
		const { data } = await this.#itemRepository.requestItems(this.selection);
		if (!data?.length) throw new Error('Source items not found');
		this.#sourceItems = data;

		const bulkMoveRepository = await createExtensionApiByAlias<UmbBulkMoveToRepository>(
			this,
			UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS,
		);
		if (!bulkMoveRepository) throw new Error('Bulk Move Repository is not available');

		await umbOpenModal(this, UMB_MOVE_TO_MODAL, {
			data: {
				unique: this.selection[0], // Use first item for entity context
				entityType: 'media',
				treeAlias: UMB_MEDIA_TREE_ALIAS,
				pickableFilter: (treeItem: UmbTreeItemModel) => !this.selection.includes(treeItem.unique as string),
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

		// Fetch destination item to get its media type
		const { data: destinationItems } = await this.#itemRepository.requestItems([destinationUnique]);
		if (!destinationItems?.length) {
			return { valid: false, error: this.#localize.term('general_error') };
		}
		const destinationItem = destinationItems[0];

		// Get allowed children of the destination's media type
		const { data: allowedChildren } = await this.#structureRepository.requestAllowedChildrenOf(
			destinationItem.mediaType.unique,
			destinationUnique,
		);

		if (!allowedChildren?.items) {
			return { valid: false, error: this.#localize.term('general_error') };
		}

		const allowedTypeUniques = new Set(allowedChildren.items.map((item) => item.unique));

		// Check which items are NOT allowed
		const invalidItems = this.#sourceItems.filter((item) => !allowedTypeUniques.has(item.mediaType.unique));

		if (invalidItems.length > 0) {
			// Get names of invalid items (use variant name or fallback)
			const invalidNames = invalidItems.map((item) => item.variants?.[0]?.name || item.name || item.unique);

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

export { UmbMoveMediaBulkAction as api };
