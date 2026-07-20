import { UmbEntityBulkActionBase } from '../../entity-bulk-action-base.js';
import { UmbEntityBulkActionProgressController } from '../../progress/index.js';
import type { UmbBulkDuplicateToRepository } from './duplicate-to-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/tree';
import type { MetaEntityBulkActionDuplicateToKind } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDuplicateToEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionDuplicateToKind> {
	#searchConfig() {
		const alias = this.args.meta.searchProviderAlias;
		return alias ? { providerAlias: alias } : undefined;
	}

	async execute() {
		if (this.selection?.length === 0) return;

		const value = await umbOpenModal(this, UMB_TREE_PICKER_MODAL, {
			data: {
				headline: '#actions_copyTo',
				confirmLabel: '#general_copy',
				foldersOnly: this.args.meta.foldersOnly,
				hideTreeRoot: this.args.meta.hideTreeRoot,
				treeAlias: this.args.meta.treeAlias,
				search: this.#searchConfig(),
			},
		}).catch(() => undefined);

		if (!value?.selection?.length) return;

		const destinationUnique = value.selection[0];
		if (destinationUnique === undefined) throw new Error('Destination Unique is not available');

		const bulkDuplicateRepository = await createExtensionApiByAlias<UmbBulkDuplicateToRepository>(
			this,
			this.args.meta.bulkDuplicateRepositoryAlias,
		);
		if (!bulkDuplicateRepository) throw new Error('Bulk Duplicate Repository is not available');

		const localize = new UmbLocalizationController(this);
		await new UmbEntityBulkActionProgressController(this).runIndeterminate({
			headline: localize.term('actions_copyInProgress'),
			operation: bulkDuplicateRepository.requestBulkDuplicateTo({
				uniques: this.selection,
				destination: { unique: destinationUnique },
			}),
			delayMs: 400,
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

export { UmbDuplicateToEntityBulkAction as api };
