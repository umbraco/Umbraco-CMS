import { UMB_WORKSPACE_CONTEXT } from '../contexts/index.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
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

export type WorkspaceEntityTypeConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceEntityType'> & {
	/**
	 * Define the workspace that this extension should be available in
	 * @example
	 * "Document"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Entity Type Condition',
	alias: 'Umb.Condition.WorkspaceEntityType',
	api: UmbWorkspaceEntityTypeCondition,
};
