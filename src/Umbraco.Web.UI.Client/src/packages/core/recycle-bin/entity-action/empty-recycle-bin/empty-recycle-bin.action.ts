import type { UmbRecycleBinRepository } from '../../recycle-bin-repository.interface.js';
import type { MetaEntityActionEmptyRecycleBinKind } from './types.js';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';

/**
 * Entity action for emptying the recycle bin.
 * @class UmbEmptyRecycleBinEntityAction
 * @augments {UmbEntityActionBase<MetaEntityActionEmptyRecycleBinKind>}
 */
export class UmbEmptyRecycleBinEntityAction extends UmbEntityActionBase<MetaEntityActionEmptyRecycleBinKind> {
	/**
	 * Executes the action.
	 * @memberof UmbEmptyRecycleBinEntityAction
	 */
	override async execute() {
		await umbConfirmModal(this._host, {
			headline: `#actions_emptyrecyclebin`,
			content: `#defaultdialogs_recycleBinWarning`,
			color: 'danger',
			confirmLabel: '#actions_emptyrecyclebin',
		});

		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.args.meta.recycleBinRepositoryAlias,
		);

		const { error } = await recycleBinRepository.requestEmpty();
		if (error) {
			throw error;
		}

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) throw new Error('Action event context is not available');
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export { UmbEmptyRecycleBinEntityAction as api };
