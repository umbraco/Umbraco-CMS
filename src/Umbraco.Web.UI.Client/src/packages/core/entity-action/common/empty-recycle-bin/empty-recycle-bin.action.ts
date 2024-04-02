import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import {
	createExtensionApiByAlias,
	type MetaEntityActionEmptyRecycleBinKind,
} from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbEmptyRecycleBinEntityAction extends UmbEntityActionBase<MetaEntityActionEmptyRecycleBinKind> {
	async execute() {
		await umbConfirmModal(this._host, {
			headline: `Empty Recycle Bin`,
			content: `When items are deleted from the recycle bin, they will be gone forever.`,
			color: 'danger',
			confirmLabel: 'Empty Recycle Bin',
		});

		const recycleBinRepository = await createExtensionApiByAlias<any>(this, this.args.meta.recycleBinRepositoryAlias);
		await recycleBinRepository.requestEmpty();

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export { UmbEmptyRecycleBinEntityAction as api };
