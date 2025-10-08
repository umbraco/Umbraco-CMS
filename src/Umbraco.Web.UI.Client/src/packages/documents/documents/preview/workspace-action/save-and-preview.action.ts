import { UmbDocumentUserPermissionCondition } from '../../user-permissions/document/conditions/document-user-permission.condition.js';
import { UMB_USER_PERMISSION_DOCUMENT_UPDATE } from '../../user-permissions/document/constants.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentSaveAndPreviewWorkspaceAction extends UmbWorkspaceActionBase {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		/* The action is disabled by default because the onChange callback
		 will first be triggered when the condition is changed to permitted */
		this.disable();

		const condition = new UmbDocumentUserPermissionCondition(host, {
			host,
			config: {
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE],
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

export { UmbDocumentSaveAndPreviewWorkspaceAction as api };
