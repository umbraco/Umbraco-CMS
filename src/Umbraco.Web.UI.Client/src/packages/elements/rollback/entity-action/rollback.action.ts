import { UMB_ELEMENT_ROLLBACK_MODAL } from '../constants.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbRollbackElementEntityAction extends UmbEntityActionBase<never> {
	#localize = new UmbLocalizationController(this);

	override async execute() {
		await umbOpenModal(this, UMB_ELEMENT_ROLLBACK_MODAL, {});
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!notificationContext) {
			throw new Error('Notification context not found');
		}
		notificationContext.peek('positive', {
			data: { message: this.#localize.term('rollback_elementRolledBack') },
		});
	}
}

export { UmbRollbackElementEntityAction as api };
