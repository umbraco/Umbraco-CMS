import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import { createExtensionApiByAlias, type MetaEntityActionDeleteKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbDeleteEntityAction extends UmbEntityActionBase<MetaEntityActionDeleteKind> {
	// TODO: make base type for item and detail models

	async execute() {
		if (!this.args.unique) throw new Error('Cannot trash an item without a unique identifier.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this.args.meta.itemRepositoryAlias,
		);

		const { data } = await itemRepository.requestItems([this.args.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		// TODO: handle items with variants
		await umbConfirmModal(this._host, {
			headline: `Trash`,
			content: `Are you sure you want to move ${item.name} to the recycle bin?`,
			color: 'danger',
			confirmLabel: 'Trash',
		});

		const recycleBinRepository = await createExtensionApiByAlias(this, this.args.meta.recycleBinRepositoryAlias);
		await recycleBinRepository.trash(this.args.unique);

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		// TODO: reload destination
	}
}
export default UmbDeleteEntityAction;
