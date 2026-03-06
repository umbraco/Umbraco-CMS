import { UMB_CONTENT_ROLLBACK_MODAL } from '../modal/constants.js';
import type { MetaEntityActionContentRollbackKind } from './types.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbContentRollbackEntityAction extends UmbEntityActionBase<MetaEntityActionContentRollbackKind> {
	#localize = new UmbLocalizationController(this);

	override async execute() {
		const result = await umbOpenModal(this, UMB_CONTENT_ROLLBACK_MODAL, {
			data: {
				rollbackRepositoryAlias: this.args.meta.rollbackRepositoryAlias,
				detailRepositoryAlias: this.args.meta.detailRepositoryAlias,
			},
		}).catch(() => undefined);

		if (!result) return;

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
