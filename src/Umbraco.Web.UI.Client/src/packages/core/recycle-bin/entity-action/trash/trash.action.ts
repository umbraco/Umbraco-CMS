import type { UmbRecycleBinRepository } from '../../recycle-bin-repository.interface.js';
import type { MetaEntityActionTrashKind } from './types.js';
import { UmbEntityTrashedEvent } from './trash.event.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

/**
 * Entity action for trashing an item.
 * @class UmbTrashEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionTrashKind>}
 */
export class UmbTrashEntityAction<
	MetaKindType extends MetaEntityActionTrashKind = MetaEntityActionTrashKind,
> extends UmbEntityActionBase<MetaKindType> {
	#localize = new UmbLocalizationController(this);

	/**
	 * Executes the action.
	 * @memberof UmbTrashEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot trash an item without a unique identifier.');

		const item = await this.#requestItem();

		await this._confirmTrash(item);

		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.args.meta.recycleBinRepositoryAlias,
		);

		const { error } = await recycleBinRepository.requestTrash({ unique: this.args.unique });
		if (error) {
			throw error;
		}

		this.#notify();
	}

	protected async _confirmTrash(item: any) {
		const headline = '#actions_trash';
		const message = '#defaultdialogs_confirmTrash';

		// TODO: handle items with variants
		await umbConfirmModal(this, {
			headline,
			content: this.#localize.string(message, item.name),
			color: 'danger',
			confirmLabel: '#actions_trash',
		});
	}

	async #requestItem() {
		if (!this.args.unique) throw new Error('Cannot trash an item without a unique identifier.');

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
		if (!actionEventContext) throw new Error('Action event context is missing.');

		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);

		const trashedEvent = new UmbEntityTrashedEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(trashedEvent);
	}
}

export { UmbTrashEntityAction as api };
