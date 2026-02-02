import type { UmbRecycleBinRepository } from '../../recycle-bin-repository.interface.js';
import { UmbEntityTrashedEvent } from '../trash/trash.event.js';
import type { UmbFolderModel } from '../../../tree/types.js';
import type { MetaEntityActionTrashFolderKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

/**
 * Entity action for trashing an item.
 * @class UmbTrashFolderEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionTrashFolderKind>}
 */
export class UmbTrashFolderEntityAction<
	MetaKindType extends MetaEntityActionTrashFolderKind = MetaEntityActionTrashFolderKind,
> extends UmbEntityActionBase<MetaKindType> {
	#localize = new UmbLocalizationController(this);

	/**
	 * Executes the action.
	 * @memberof UmbTrashFolderEntityAction
	 */
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot trash a folder without a unique identifier.');

		const folder = await this.#requestFolder();

		await this._confirmTrash(folder);

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

	protected async _confirmTrash(folder: UmbFolderModel) {
		const headline = '#actions_trash';
		const message = '#defaultdialogs_confirmTrash';

		// TODO: handle items with variants
		await umbConfirmModal(this, {
			headline,
			content: this.#localize.string(message, folder.name),
			color: 'danger',
			confirmLabel: '#actions_trash',
		});
	}

	async #requestFolder() {
		if (!this.args.unique) throw new Error('Cannot trash a folder without a unique identifier.');

		const folderRepository = await createExtensionApiByAlias<UmbDetailRepositoryBase<UmbFolderModel>>(
			this,
			this.args.meta.folderRepositoryAlias,
		);

		const { data: folder } = await folderRepository.requestByUnique(this.args.unique);
		if (!folder) throw new Error('Folder not found.');
		return folder;
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

export { UmbTrashFolderEntityAction as api };
