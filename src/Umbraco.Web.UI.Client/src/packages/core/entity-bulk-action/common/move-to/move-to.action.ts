import type { UmbBulkMoveToRepository } from './move-to-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/tree';
import type { MetaEntityBulkActionMoveToKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbMediaMoveEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionMoveToKind> {
	async execute() {
		if (this.selection?.length === 0) return;

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				foldersOnly: this.args.meta.foldersOnly,
				hideTreeRoot: this.args.meta.hideTreeRoot,
				treeAlias: this.args.meta.treeAlias,
			},
		});

		if (!value?.selection?.length) return;

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const bulkMoveRepository = await createExtensionApiByAlias<UmbBulkMoveToRepository>(
			this,
			this.args.meta.bulkMoveRepositoryAlias,
		);
		if (!bulkMoveRepository) throw new Error('Bulk Move Repository is not available');

		await bulkMoveRepository.requestBulkMoveTo({ uniques: this.selection, destination: { unique: destinationUnique } });

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

export { UmbMediaMoveEntityBulkAction as api };
