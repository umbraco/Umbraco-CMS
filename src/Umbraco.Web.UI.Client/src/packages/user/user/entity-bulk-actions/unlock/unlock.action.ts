import { UmbUnlockUserRepository } from '../../repository/index.js';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';

export class UmbUnlockUserEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const repository = new UmbUnlockUserRepository(this._host);
		await repository.unlock(this.selection);

		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Entity unique not found');

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType,
			unique,
		});

		eventContext.dispatchEvent(event);
	}
}

export { UmbUnlockUserEntityBulkAction as api };
