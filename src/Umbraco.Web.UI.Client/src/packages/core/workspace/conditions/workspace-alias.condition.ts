import { UMB_WORKSPACE_CONTEXT, type UmbWorkspaceContext } from '../contexts/index.js';
import type { WorkspaceAliasConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbWorkspaceAliasCondition
	extends UmbConditionBase<WorkspaceAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceAliasConditionConfig>) {
		super(host, args);

		let permissionCheck: ((context: UmbWorkspaceContext) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (context: UmbWorkspaceContext) => context.workspaceAlias === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (context: UmbWorkspaceContext) => this.config.oneOf!.indexOf(context.workspaceAlias) !== -1;
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
				this.permitted = permissionCheck!(context);
			});
		} else {
			throw new Error(
				'Condition `Umb.Condition.WorkspaceAlias` could not be initialized properly. Either "match" or "oneOf" must be defined',
			);
		}
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Alias Condition',
	alias: 'Umb.Condition.WorkspaceAlias',
	api: UmbWorkspaceAliasCondition,
};
