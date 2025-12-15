import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbMediaTreeItemModel } from '../../tree/types.js';
import { UmbMediaItemRepository } from '../../repository/index.js';
import { UMB_MEDIA_TREE_ALIAS } from '../../constants.js';
import { UMB_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import type { UmbMoveRepository, UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';
import { UMB_MOVE_TO_MODAL } from '@umbraco-cms/backoffice/tree';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbMoveMediaEntityAction extends UmbEntityActionBase<never> {
	#localize = new UmbLocalizationController(this);
	#moveRepository?: UmbMoveRepository;
	#itemRepository = new UmbMediaItemRepository(this);
	#structureRepository = new UmbMediaTypeStructureRepository(this);
	#sourceItem?: UmbMediaItemModel;
	#disallowedMediaTypes = new Set<string>();

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		this.#moveRepository = await createExtensionApiByAlias<UmbMoveRepository>(this, UMB_MOVE_MEDIA_REPOSITORY_ALIAS);
		if (!this.#moveRepository) throw new Error('Move Repository is not available');

		// Fetch source item to get its media type
		const { data } = await this.#itemRepository.requestItems([this.args.unique]);
		if (!data?.length) throw new Error('Source item not found');
		this.#sourceItem = data[0];

		await umbOpenModal(this, UMB_MOVE_TO_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				treeAlias: UMB_MEDIA_TREE_ALIAS,
				name: this.#sourceItem.name,
				pickableFilter: (treeItem: UmbTreeItemModel) => {
					if (treeItem.unique === this.args.unique) return false;
					const mediaType = (treeItem as UmbMediaTreeItemModel).mediaType?.unique;
					if (mediaType && this.#disallowedMediaTypes.has(mediaType)) return false;
					return true;
				},
				onSelection: async (destinationUnique: string | null) => this.#onSelection(destinationUnique),
				onBeforeSubmit: async (destinationUnique: string | null) => this.#onBeforeSubmit(destinationUnique),
			},
		});

		this.#reloadMenu();
	}

	async #onSelection(destinationUnique: string | null): Promise<{ valid: boolean; error?: string }> {
		if (!this.#sourceItem) {
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

		// Check if source media type is allowed
		const isAllowed = allowedChildren.items.some((allowed) => allowed.unique === this.#sourceItem!.mediaType.unique);

		if (!isAllowed) {
			// Add to disallowed types so all items of this type become unselectable
			this.#disallowedMediaTypes.add(destinationItem.mediaType.unique);
			return {
				valid: false,
				error: this.#localize.term('moveOrCopy_notAllowedByContentType'),
			};
		}

		return { valid: true };
	}

	async #onBeforeSubmit(destinationUnique: string | null): Promise<{ success: boolean; error?: { message: string } }> {
		if (!this.#moveRepository) {
			return { success: false, error: { message: 'Move Repository is not available' } };
		}

		if (!this.args.unique) {
			return { success: false, error: { message: 'Unique is not available' } };
		}

		const { error } = await this.#moveRepository.requestMoveTo({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
		});

		if (error) {
			return { success: false, error: { message: error.message } };
		}

		return { success: true };
	}

	async #reloadMenu() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) throw new Error('Action Event Context is not available');
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export { UmbMoveMediaEntityAction as api };
