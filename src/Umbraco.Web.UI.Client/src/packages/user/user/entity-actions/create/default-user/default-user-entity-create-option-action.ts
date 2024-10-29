import { UMB_CREATE_USER_MODAL } from '../../../modals/create/create-user-modal.token.js';
import type { UmbUserKindType } from '../../../utils/index.js';
import { UmbUserKind } from '../../../utils/index.js';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbEntityCreateOptionActionBase,
	UmbRequestReloadChildrenOfEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbDefaultUserEntityCreateOptionAction extends UmbEntityCreateOptionActionBase<never> {
	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const kind: UmbUserKindType = UmbUserKind.DEFAULT;

		const modalContext = modalManager.open(this, UMB_CREATE_USER_MODAL, {
			data: {
				user: {
					kind,
				},
			},
		});

		await modalContext
			?.onSubmit()
			.then(() => {
				this.#requestReloadChildrenOfEntity();
			})
			.catch(async () => {
				// modal is closed after creation instead of navigating to the new user.
				// We therefore need to reload the children of the entity
				this.#requestReloadChildrenOfEntity();
			});
	}

	async #requestReloadChildrenOfEntity() {
		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: this.args.entityType,
			unique: this.args.unique,
		});

		eventContext.dispatchEvent(event);
	}
}

export { UmbDefaultUserEntityCreateOptionAction as api };
