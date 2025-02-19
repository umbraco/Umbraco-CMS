import type { MetaEntityBulkActionDeleteKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbEntityDeletedEvent,
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbDeleteEntityBulkAction<
	MetaKindType extends MetaEntityBulkActionDeleteKind = MetaEntityBulkActionDeleteKind,
> extends UmbEntityBulkActionBase<MetaKindType> {
	#localize = new UmbLocalizationController(this);
	_items: Array<any> = [];

	override async execute() {
		if (this.selection?.length === 0) {
			throw new Error('No items selected.');
		}

		// TODO: Move item look up to a future bulk action context
		await this.#requestItems();
		await this._confirmDelete(this._items);
		await this.#requestBulkDelete(this.selection);
	}

	protected async _confirmDelete(items: Array<any>) {
		const headline = '#actions_delete';
		const message = '#defaultdialogs_confirmBulkDelete';

		// TODO: consider showing more details about the items being deleted
		await umbConfirmModal(this._host, {
			headline,
			content: this.#localize.string(message, items.length),
			color: 'danger',
			confirmLabel: '#actions_delete',
		});
	}

	async #requestItems() {
		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this.args.meta.itemRepositoryAlias,
		);

		const { data } = await itemRepository.requestItems(this.selection);

		this._items = data ?? [];
	}

	async #requestBulkDelete(uniques: Array<string>) {
		const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<UmbEntityModel>>(
			this,
			this.args.meta.detailRepositoryAlias,
		);

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		const succeeded: Array<string> = [];

		for (const unique of uniques) {
			const { error } = await detailRepository.delete(unique);

			if (error) {
				const notification = { data: { message: error.message } };
				notificationContext?.peek('danger', notification);
			} else {
				succeeded.push(unique);
			}
		}

		if (succeeded.length > 0) {
			const notification = {
				data: { message: `Deleted ${succeeded.length} ${succeeded.length === 1 ? 'item' : 'items'}` },
			};
			notificationContext?.peek('positive', notification);
		}

		await this.#notify(succeeded);
	}

	async #notify(succeeded: Array<string>) {
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) throw new Error('Entity Context is not available');

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) throw new Error('Event Context is not available');

		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (entityType && unique !== undefined) {
			const args = { entityType, unique };

			const reloadChildren = new UmbRequestReloadChildrenOfEntityEvent(args);
			eventContext.dispatchEvent(reloadChildren);

			const reloadStructure = new UmbRequestReloadStructureForEntityEvent(args);
			eventContext.dispatchEvent(reloadStructure);
		}

		const succeededItems = this._items.filter((item) => succeeded.includes(item.unique));

		succeededItems.forEach((item) => {
			const deletedEvent = new UmbEntityDeletedEvent({
				unique: item.unique,
				entityType: item.entityType,
			});

			eventContext.dispatchEvent(deletedEvent);
		});
	}
}

export { UmbDeleteEntityBulkAction as api };
