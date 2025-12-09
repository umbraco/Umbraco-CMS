import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import type { MetaEntityActionDeleteKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_DELETE_MODAL } from './modal/entity-delete-modal.token.js';

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

		const { error } = await detailRepository.delete(this.args.unique);
		if (error) {
			throw error;
		}

		await this.#notify();
	}

	protected async _confirmDelete(item: any) {
		await umbOpenModal(this, UMB_ENTITY_DELETE_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				itemRepositoryAlias: this.args.meta.itemRepositoryAlias,
				headline: this.args.meta.confirm?.headline,
				message: this.args.meta.confirm?.message,
			},
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
