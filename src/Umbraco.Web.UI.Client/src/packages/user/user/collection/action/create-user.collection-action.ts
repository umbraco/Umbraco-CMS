import { UMB_CREATE_USER_MODAL } from '../../modals/create/create-user-modal.token.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateUserCollectionAction extends UmbControllerBase {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);

		const unique = entityContext.getUnique();
		const entityType = entityContext.getEntityType();

		if (unique === undefined) throw new Error('Missing unique');
		if (!entityType) throw new Error('Missing entityType');

		const modalContext = modalManager.open(this, UMB_CREATE_USER_MODAL);
		modalContext?.onSubmit().catch(async () => {
			// modal is closed after creation instead of navigating to the new user.
			// We therefore need to reload the children of the entity
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType,
				unique,
			});

			eventContext.dispatchEvent(event);
		});
	}
}
