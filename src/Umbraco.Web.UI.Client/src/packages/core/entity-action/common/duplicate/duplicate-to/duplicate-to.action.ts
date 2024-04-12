import { UmbEntityActionBase } from '../../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../../request-reload-structure-for-entity.event.js';
import type { UmbDuplicateRepository } from '../duplicate-repository.interface.js';
import { UMB_DUPLICATE_MODAL } from '../modal/duplicate-modal.token.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { MetaEntityActionDuplicateToKind } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDuplicateToEntityAction extends UmbEntityActionBase<MetaEntityActionDuplicateToKind> {
	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_DUPLICATE_MODAL);
		debugger;

		/*
		const value = await modalContext.onSubmit();
		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');
		*/

		const duplicateRepository = await createExtensionApiByAlias<UmbDuplicateRepository>(
			this,
			this.args.meta.duplicateRepositoryAlias,
		);
		if (!duplicateRepository) throw new Error('Duplicate repository is not available');

		//alert('Duplicate to: ' + destinationUnique);

		/*
		await duplicateRepository.requestDuplicateTo({
			unique: this.args.unique,
			destination: { unique: destinationUnique },
		});
		*/

		//this.#reloadMenu();
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
