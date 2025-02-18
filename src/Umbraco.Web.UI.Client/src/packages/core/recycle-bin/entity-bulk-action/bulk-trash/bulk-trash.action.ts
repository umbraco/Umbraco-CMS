import type { UmbRecycleBinRepository } from '../../recycle-bin-repository.interface.js';
import { UmbEntityTrashedEvent } from '../../entity-action/trash/index.js';
import type { MetaEntityBulkActionTrashKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export class UmbTrashEntityBulkAction<
	MetaKindType extends MetaEntityBulkActionTrashKind = MetaEntityBulkActionTrashKind,
> extends UmbEntityBulkActionBase<MetaKindType> {
	override async execute() {
		if (this.selection?.length === 0) {
			throw new Error('No items selected.');
		}

		await this._confirmTrash();
		await this.#requestBulkTrash(this.selection);
		await this.#notify();
	}

	protected async _confirmTrash() {
		const headline = '#actions_trash';

		// TODO: handle items with variants
		await umbConfirmModal(this._host, {
			headline,
			content: `Are you sure you want to move ${this.selection.length} ${this.selection.length === 1 ? 'item' : 'items'} to the recycle bin?`,
			color: 'danger',
			confirmLabel: '#actions_trash',
		});
	}

	async #requestBulkTrash(uniques: Array<string>) {
		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.args.meta.recycleBinRepositoryAlias,
		);

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		let count = 0;

		for (const unique of uniques) {
			const { error } = await recycleBinRepository.requestTrash({ unique });

			if (error) {
				const notification = { data: { message: error.message } };
				notificationContext?.peek('danger', notification);
			} else {
				count++;
			}
		}

		if (count > 0) {
			const notification = { data: { message: `Trashed ${count} ${count === 1 ? 'item' : 'items'}` } };
			notificationContext?.peek('positive', notification);
		}

		return {};
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

export { UmbTrashEntityBulkAction as api };
