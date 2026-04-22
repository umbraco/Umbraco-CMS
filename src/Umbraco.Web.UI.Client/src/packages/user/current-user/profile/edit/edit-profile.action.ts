import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from '../../../section/constants.js';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '../../current-user-action.extension.js';
import { UMB_CURRENT_USER_EDIT_PROFILE_MODAL } from './edit-profile-modal.token.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import { UMB_USER_WORKSPACE_PATH } from '@umbraco-cms/backoffice/user';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEditProfileCurrentUserAction<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	#hasAccessToUserSection? = false;
	#init: Promise<typeof UMB_CURRENT_USER_CONTEXT.TYPE | undefined>;
	#unique?: string;

	constructor(host: UmbControllerHost, args: UmbCurrentUserActionArgs<ArgsMetaType>) {
		super(host, args);

		this.#init = this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => {
					this.#unique = currentUser?.unique;
					this.#hasAccessToUserSection = currentUser?.allowedSections?.includes(UMB_USER_MANAGEMENT_SECTION_ALIAS);
				},
				'umbEditProfileCurrentUserActionObserver',
			);
		}).asPromise();
	}

	async getHref() {
		await this.#init;
		if (!this.#hasAccessToUserSection) return;
		return `${UMB_USER_WORKSPACE_PATH}/edit/${this.#unique}`;
	}

	async execute() {
		await this.#init;
		if (this.#hasAccessToUserSection) return;
		await umbOpenModal(this, UMB_CURRENT_USER_EDIT_PROFILE_MODAL);
	}
}

export { UmbEditProfileCurrentUserAction as api };
