import { UMB_ROLLBACK_MODAL } from '../constants.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbRollbackDocumentEntityAction extends UmbEntityActionBase<never> {
	#localize = new UmbLocalizationController(this);

	override async execute() {
		const modalManagerContext = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManagerContext) return;

		const modalContext = modalManagerContext.open(this, UMB_ROLLBACK_MODAL, {});

		const data = await modalContext.onSubmit().catch(() => undefined);
		if (!data) return;

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!notificationContext) {
			throw new Error('Notification context not found');
		}
		notificationContext.peek('positive', {
			data: { message: this.#localize.term('rollback_documentRolledBack') },
		});
	}
}

export { UmbRollbackDocumentEntityAction as api };
