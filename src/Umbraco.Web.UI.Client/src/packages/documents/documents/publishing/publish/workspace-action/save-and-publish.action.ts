import { UmbDocumentUserPermissionCondition } from '../../../user-permissions/document/conditions/document-user-permission.condition.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PUBLISH,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../../user-permissions/document/constants.js';
import { UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT } from '../../workspace-context/constants.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../../constants.js';
import { UmbWorkspaceActionBase, type UmbWorkspaceActionArgs } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentSaveAndPublishWorkspaceAction extends UmbWorkspaceActionBase {
	constructor(host: UmbControllerHost, args: UmbWorkspaceActionArgs<never>) {
		super(host, args);

		/* The action is disabled by default because the onChange callback
		 will first be triggered when the condition is changed to permitted */
		this.disable();

		new UmbDocumentUserPermissionCondition(host, {
			host,
			config: {
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_UPDATE, UMB_USER_PERMISSION_DOCUMENT_PUBLISH],
			},
			onChange: (permitted: boolean) => {
				if (permitted) {
					this.enable();
				} else {
					this.disable();
				}
			},
		});
	}

	async hasAdditionalOptions() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('The workspace context is missing');
		}
		const variantOptions = await this.observe(workspaceContext.variantOptions).asPromise();
		const cultureVariantOptions = variantOptions?.filter((option) => option.segment === null);
		return cultureVariantOptions?.length > 1;
	}

	override async execute() {
		const workspaceContext = await this.getContext(UMB_DOCUMENT_PUBLISHING_WORKSPACE_CONTEXT);
		if (!workspaceContext) {
			throw new Error('The workspace context is missing');
		}
		return workspaceContext.saveAndPublish();
	}
}

export { UmbDocumentSaveAndPublishWorkspaceAction as api };
