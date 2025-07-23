import { UmbDocumentUserPermissionCondition } from '../../user-permissions/document/conditions/document-user-permission.condition.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../document-workspace.context-token.js';
import { UMB_USER_PERMISSION_DOCUMENT_UPDATE } from '../../user-permissions/document/constants.js';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// TODO: Investigate how additional preview environments can be supported. [LK:2024-05-16]
// https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/additional-preview-environments-support
// In v13, they are registered on the server using `SendingContentNotification`, which is no longer available in v14.

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

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('Document workspace context not found');
		}
		workspaceContext.saveAndPreview();
	}
}

export { UmbDocumentSaveAndPreviewWorkspaceAction as api };
