import { UmbDisableUserRepository } from '../../repository/index.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityBulkActionBase } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

export class UmbDisableUserEntityBulkAction extends UmbEntityBulkActionBase<object> {
	async execute() {
		const repository = new UmbDisableUserRepository(this._host);
		await repository.disable(this.selection);

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

export { UmbDisableUserEntityBulkAction as api };
