import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../document-workspace.context-token.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../user-permissions/constants.js';
import { UmbDocumentUserPermissionCondition } from '../../user-permissions/condition/document-user-permission.condition.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbDocumentSaveAndPublishWorkspaceAction extends UmbWorkspaceActionBase {
	constructor(host: UmbControllerHost, args: any) {
		super(host, args);

		/* The action is disabled by default because the onChange callback
		 will first be triggered when the condition is changed to permitted */
		this.disable();

		const condition = new UmbDocumentUserPermissionCondition(host, {
			host,
			config: {
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE, UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			onChange: () => {
				condition.permitted ? this.enable() : this.disable();
			},
		});
	}

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		return workspaceContext.saveAndPublish();
	}
}

export { UmbDocumentSaveAndPublishWorkspaceAction as api };
