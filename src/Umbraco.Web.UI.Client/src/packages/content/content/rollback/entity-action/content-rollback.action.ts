import type { UmbRollbackModalData, UmbRollbackModalValue } from '../modal/types.js';
import type { MetaEntityActionRollbackKind } from './types.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbModalToken, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbContentRollbackEntityAction extends UmbEntityActionBase<MetaEntityActionRollbackKind> {
	#localize = new UmbLocalizationController(this);

	override async execute() {
		const token = new UmbModalToken<UmbRollbackModalData, UmbRollbackModalValue>(this.args.meta.rollbackModalAlias, {
			modal: {
				type: 'sidebar',
				size: 'full',
			},
		});

		await umbOpenModal(this, token, {});

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!notificationContext) {
			throw new Error('Notification context not found');
		}
		notificationContext.peek('positive', {
			data: { message: this.#localize.term('rollback_documentRolledBack') },
		});
	}
}

export { UmbContentRollbackEntityAction as api };
