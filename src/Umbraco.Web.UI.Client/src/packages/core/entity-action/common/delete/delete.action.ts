import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import type { MetaEntityActionDeleteKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbDeleteEntityAction<
	MetaKind extends MetaEntityActionDeleteKind = MetaEntityActionDeleteKind,
> extends UmbEntityActionBase<MetaKind> {
	// TODO: make base type for item and detail models
	#localize = new UmbLocalizationController(this);

	override async execute() {
		if (!this.args.unique) throw new Error('Cannot delete an item without a unique identifier.');

		const item = await this.#requestItem();

		await this._confirmDelete(item);

		const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<any>>(
			this,
			this.args.meta.detailRepositoryAlias,
		);

		await detailRepository.delete(this.args.unique);

		await this.#notify();
	}

	async _confirmDelete(item: any) {
		const headline = this.args.meta.confirm?.headline ?? '#actions_delete';
		const message = this.args.meta.confirm?.message ?? '#defaultdialogs_confirmdelete';

		// TODO: handle items with variants
		await umbConfirmModal(this, {
			headline,
			content: this.#localize.string(message, item.name),
			color: 'danger',
			confirmLabel: '#general_delete',
		});
	}

	async #requestItem() {
		if (!this.args.unique) throw new Error('Cannot delete an item without a unique identifier.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this.args.meta.itemRepositoryAlias,
		);

		const { data } = await itemRepository.requestItems([this.args.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		return item;
	}

	async #notify() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context not found.');
		}

		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}
export default UmbDeleteEntityAction;
