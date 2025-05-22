import { UMB_WORKSPACE_CONTEXT } from '../workspace.context-token.js';
import type { UmbWorkspaceEntityTypeConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWorkspaceEntityTypeCondition
	extends UmbConditionBase<UmbWorkspaceEntityTypeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbWorkspaceEntityTypeConditionConfig>) {
		super(host, args);
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.permitted = context?.getEntityType().toLowerCase() === this.config.match.toLowerCase();
		});
	}
}

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Workspace Entity Type Condition',
	alias: 'Umb.Condition.WorkspaceEntityType',
	api: UmbWorkspaceEntityTypeCondition,
};
