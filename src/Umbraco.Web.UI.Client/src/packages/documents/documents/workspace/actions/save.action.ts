import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../user-permissions/constants.js';
import { UmbDocumentUserPermissionCondition } from '../../user-permissions/document-user-permission.condition.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveWorkspaceAction extends UmbSubmitWorkspaceAction {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		/* The action is disabled by default because the onChange callback
		 will first be triggered when the condition is changed to permitted */
		this.disable();

		const condition = new UmbDocumentUserPermissionCondition(host, {
			host,
			config: {
				alias: 'Umb.Condition.UserPermission.Document',
				oneOf: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_UPDATE],
			},
			onChange: () => {
				condition.permitted ? this.enable() : this.disable();
			},
		});
	}
}

export { UmbDocumentSaveWorkspaceAction as api };
