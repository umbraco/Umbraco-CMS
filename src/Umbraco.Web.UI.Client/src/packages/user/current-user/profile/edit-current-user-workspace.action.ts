import { UMB_CURRENT_USER_WORKSPACE_MODAL } from '../modals/current-user-workspace/current-user-workspace-modal.token.js';
import type { UmbCurrentUserAction, UmbCurrentUserActionArgs } from '../current-user-action.extension.js';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbEditCurrentUserWorkspaceAction<ArgsMetaType = never>
	extends UmbActionBase<UmbCurrentUserActionArgs<ArgsMetaType>>
	implements UmbCurrentUserAction<ArgsMetaType>
{
	async getHref() {
		return undefined;
	}

	async execute() {
		await umbOpenModal(this, UMB_CURRENT_USER_WORKSPACE_MODAL);
	}
}

export { UmbEditCurrentUserWorkspaceAction as api };
