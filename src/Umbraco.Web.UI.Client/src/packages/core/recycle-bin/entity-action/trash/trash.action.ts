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
export class UmbTrashEntityAction extends UmbEntityActionBase<MetaEntityActionTrashKind> {
	#localize = new UmbLocalizationController(this);

	/**
	 * Executes the action.
	 * @memberof UmbTrashEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot trash an item without a unique identifier.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this.args.meta.itemRepositoryAlias,
		);

		const { data } = await itemRepository.requestItems([this.args.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		const headline = '#actions_trash';
		const message = '#defaultdialogs_confirmtrash';

		// TODO: handle items with variants
		await umbConfirmModal(this._host, {
			headline,
			content: this.#localize.string(message, item.name),
			color: 'danger',
			confirmLabel: '#actions_trash',
		});

		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.args.meta.recycleBinRepositoryAlias,
		);

		await recycleBinRepository.requestTrash({ unique: this.args.unique });

		this.#notify();
	}

	async #notify() {
		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

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
