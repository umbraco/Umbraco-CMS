import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import type { UmbMoveRepository } from './move-repository.interface.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { MetaEntityActionMoveToKind } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbMoveToEntityAction extends UmbEntityActionBase<MetaEntityActionMoveToKind> {
	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		if (!this.args.entityType) throw new Error('Entity Type is not available');

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, this.args.meta.treePickerModal) as any; // TODO: make generic picker interface with selection
		const value = await modalContext.onSubmit();
		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const moveRepository = await createExtensionApiByAlias<UmbMoveRepository>(this, this.args.meta.moveRepositoryAlias);
		if (!moveRepository) throw new Error('Move Repository is not available');

		await moveRepository.requestMoveTo({ unique: this.args.unique, destination: { unique: destinationUnique } });

		this.#reloadMenu();
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

export default UmbMoveToEntityAction;
