import { UMB_DUPLICATE_TO_MODAL } from './modal/duplicate-to-modal.token.js';
import type { MetaEntityActionDuplicateToKind, UmbDuplicateToRepository } from './types.js';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDuplicateToEntityAction extends UmbEntityActionBase<MetaEntityActionDuplicateToKind> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_DUPLICATE_TO_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				treeAlias: this.args.meta.treeAlias,
				foldersOnly: this.args.meta.foldersOnly,
			},
		});

		try {
			const value = await modal.onSubmit();
			const destinationUnique = value.destination.unique;
			if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

			const duplicateRepository = await createExtensionApiByAlias<UmbDuplicateToRepository>(
				this,
				this.args.meta.duplicateRepositoryAlias,
			);
			if (!duplicateRepository) throw new Error('Duplicate repository is not available');

			const { error } = await duplicateRepository.requestDuplicateTo({
				unique: this.args.unique,
				destination: { unique: destinationUnique },
			});

			if (!error) {
				this.#reloadMenu();
			}
		} catch (error) {
			console.error(error);
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

export { UmbDuplicateToEntityAction as api };
