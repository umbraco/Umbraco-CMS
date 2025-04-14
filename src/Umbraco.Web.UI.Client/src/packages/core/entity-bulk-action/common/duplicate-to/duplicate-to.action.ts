import type { UmbBulkDuplicateToRepository } from './duplicate-to-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/tree';
import type { MetaEntityBulkActionDuplicateToKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityBulkActionBase } from '../../entity-bulk-action-base.js';

export class UmbMediaDuplicateEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionDuplicateToKind> {
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

		const bulkDuplicateRepository = await createExtensionApiByAlias<UmbBulkDuplicateToRepository>(
			this,
			this.args.meta.bulkDuplicateRepositoryAlias,
		);
		if (!bulkDuplicateRepository) throw new Error('Bulk Duplicate Repository is not available');

		await bulkDuplicateRepository.requestBulkDuplicateTo({
			uniques: this.selection,
			destination: { unique: destinationUnique },
		});

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

export { UmbMediaDuplicateEntityBulkAction as api };
