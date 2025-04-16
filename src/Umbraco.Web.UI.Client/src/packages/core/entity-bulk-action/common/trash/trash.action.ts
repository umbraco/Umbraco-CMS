import type { UmbBulkTrashRepository } from './trash-repository.interface.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityBulkActionBase, type UmbEntityBulkActionArgs } from '@umbraco-cms/backoffice/entity-bulk-action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { MetaEntityBulkActionTrashKind } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated since v15.3.0. Will be removed in v17.0.0. import `UmbMediaTrashEntityBulkAction` from @umbraco-cms/backoffice/recycle-bin instead.
 * @exports
 * @class UmbMediaTrashEntityBulkAction
 * @augments {UmbEntityBulkActionBase<MetaEntityBulkActionTrashKind>}
 */
export class UmbMediaTrashEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionTrashKind> {
	constructor(host: UmbControllerHost, args: UmbEntityBulkActionArgs<MetaEntityBulkActionTrashKind>) {
		super(host, args);

		new UmbDeprecation({
			removeInVersion: '17.0.0',
			deprecated: 'UmbMediaTrashEntityBulkAction',
			solution: 'import UmbMediaTrashEntityBulkAction from @umbraco-cms/backoffice/recycle-bin instead.',
		}).warn();
	}

	async execute() {
		if (this.selection?.length === 0) return;

		await umbConfirmModal(this._host, {
			headline: `Trash`,
			content: `Are you sure you want to move ${this.selection.length} ${this.selection.length === 1 ? 'item' : 'items'} to the recycle bin?`,
			color: 'danger',
			confirmLabel: 'Trash',
		});

		const bulkTrashRepository = await createExtensionApiByAlias<UmbBulkTrashRepository>(
			this,
			this.args.meta.bulkTrashRepositoryAlias,
		);
		if (!bulkTrashRepository) throw new Error('Bulk Trash Repository is not available');

		await bulkTrashRepository.requestBulkTrash({ uniques: this.selection });

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

export { UmbMediaTrashEntityBulkAction as api };
