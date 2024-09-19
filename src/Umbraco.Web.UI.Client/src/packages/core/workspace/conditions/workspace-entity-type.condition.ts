import { UMB_WORKSPACE_CONTEXT } from '../contexts/index.js';
import type { WorkspaceEntityTypeConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWorkspaceEntityTypeCondition
	extends UmbConditionBase<WorkspaceEntityTypeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceEntityTypeConditionConfig>) {
		super(host, args);
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.permitted = context.getEntityType().toLowerCase() === this.config.match.toLowerCase();
		});
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Entity Type Condition',
	alias: 'Umb.Condition.WorkspaceEntityType',
	api: UmbWorkspaceEntityTypeCondition,
};
