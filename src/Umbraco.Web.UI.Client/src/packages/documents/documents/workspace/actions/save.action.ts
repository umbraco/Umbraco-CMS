import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../user-permissions/constants.js';
import { UmbDocumentUserPermissionCondition } from '../../user-permissions/conditions/document-user-permission.condition.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveWorkspaceAction extends UmbSubmitWorkspaceAction {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		/* The action is disabled by default because the onChange callback
		 will first be triggered when the condition is changed to permitted */
		this.disable();

		// TODO: this check is not sufficient. It will show the save button if a use
		// has only create options. The best solution would be to split the two buttons into two separate actions
		// with a condition on isNew to show/hide them
		// The server will throw a permission error if this scenario happens
		const condition = new UmbDocumentUserPermissionCondition(host, {
			host,
			config: {
				alias: 'Umb.Condition.UserPermission.Document',
				oneOf: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_UPDATE],
			},
			onChange: () => {
				if (condition.permitted) {
					this.enable();
				} else {
					this.disable();
				}
			},
		});
	}
}

export { UmbDocumentSaveWorkspaceAction as api };
