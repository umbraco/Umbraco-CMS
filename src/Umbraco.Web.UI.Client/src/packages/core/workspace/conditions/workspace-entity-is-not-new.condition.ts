import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../index.js';
import type { WorkspaceEntityIsNotNewConditionConfig } from './types.js';
import { UMB_WORKSPACE_ENTITY_IS_NOT_NEW_CONDITION_ALIAS } from './const.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbWorkspaceEntityIsNotNewCondition
	extends UmbConditionBase<WorkspaceEntityIsNotNewConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceEntityIsNotNewConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.isNew,
				(isNew) => {
					if (isNew !== undefined) {
						// Check if equal to match, if match not set it defaults to false.
						this.permitted = isNew === (this.config.match !== undefined ? this.config.match : false);
					}
				},
				ObserveSymbol,
			);
		});
	}
}

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Workspace Entity Is Not New Condition',
	alias: UMB_WORKSPACE_ENTITY_IS_NOT_NEW_CONDITION_ALIAS,
	api: UmbWorkspaceEntityIsNotNewCondition,
};