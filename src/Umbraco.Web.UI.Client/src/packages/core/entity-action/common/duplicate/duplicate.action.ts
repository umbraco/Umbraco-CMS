import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import type { UmbDuplicateRepository } from './duplicate-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDuplicateEntityAction extends UmbEntityActionBase<any> {
	override async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		try {
			const duplicateRepository = await createExtensionApiByAlias<UmbDuplicateRepository>(
				this,
				this.args.meta.duplicateRepositoryAlias,
			);
			if (!duplicateRepository) throw new Error('Duplicate repository is not available');

			const { error } = await duplicateRepository.requestDuplicate({
				unique: this.args.unique,
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
	}
}

export { UmbDuplicateEntityAction as api };
