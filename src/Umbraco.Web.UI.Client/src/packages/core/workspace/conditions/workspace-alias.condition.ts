import { UMB_WORKSPACE_CONTEXT } from '../workspace.context-token.js';
import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { WorkspaceAliasConditionConfig } from './types.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from './const.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
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
				if (context) {
					this.permitted = permissionCheck!(context);
				} else {
					this.permitted = false;
				}
			});
		} else {
			throw new Error(
				`Condition [UMB_WORKSPACE_CONDITION_ALIAS] (${UMB_WORKSPACE_CONDITION_ALIAS}) could not be initialized properly. Either "match" or "oneOf" must be defined.`,
			);
		}
	}
}

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Workspace Alias Condition',
	alias: UMB_WORKSPACE_CONDITION_ALIAS,
	api: UmbWorkspaceAliasCondition,
};
