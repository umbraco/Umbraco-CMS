import { UMB_CONTENT_ROLLBACK_MODAL } from '../modal/constants.js';
import type { MetaEntityActionContentRollbackKind } from './types.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

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

		const localizationKey = result.entityType
			? `rollback_${result.entityType}RolledBack`
			: 'rollback_contentRolledBack';
		const message = this.#localize.term(localizationKey);

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		notificationContext?.peek('positive', { data: { message } });
	}
}

export { UmbContentRollbackEntityAction as api };
