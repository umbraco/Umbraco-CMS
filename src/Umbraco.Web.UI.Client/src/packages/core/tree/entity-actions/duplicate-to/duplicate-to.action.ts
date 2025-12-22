import { UMB_DUPLICATE_TO_MODAL } from './modal/duplicate-to-modal.token.js';
import type { MetaEntityActionDuplicateToKind, UmbDuplicateToRepository } from './types.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDuplicateToEntityAction extends UmbEntityActionBase<MetaEntityActionDuplicateToKind> {
	#duplicateRepository?: UmbDuplicateToRepository;

	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		this.#duplicateRepository = await createExtensionApiByAlias<UmbDuplicateToRepository>(
			this,
			this.args.meta.duplicateRepositoryAlias,
		);
		if (!this.#duplicateRepository) throw new Error('Duplicate repository is not available');

		await umbOpenModal(this, UMB_DUPLICATE_TO_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
				pickableFilter: (treeItem) => treeItem.unique !== this.args.unique,
				onBeforeSubmit: async (destinationUnique) => this.#onBeforeSubmit(destinationUnique),
			},
		});

		this.#reloadMenu();
	}

	async #onBeforeSubmit(destinationUnique: string | null): Promise<{ success: boolean; error?: { message: string } }> {
		if (!this.#duplicateRepository) {
			return { success: false, error: { message: 'Duplicate repository is not available' } };
		}

		if (!this.args.unique) {
			return { success: false, error: { message: 'Unique is not available' } };
		}

		const { error } = await this.#duplicateRepository.requestDuplicateTo({
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
		if (!actionEventContext) throw new Error('Action event context is not available');
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		// TODO: Reload destination
	}
}

export { UmbDuplicateToEntityAction as api };
