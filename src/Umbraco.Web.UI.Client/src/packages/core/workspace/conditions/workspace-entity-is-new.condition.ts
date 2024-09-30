import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '../index.js';
import type { WorkspaceEntityIsNewConditionConfig } from './types.js';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION } from './const.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbWorkspaceEntityIsNewCondition
	extends UmbConditionBase<WorkspaceEntityIsNewConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceEntityIsNewConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.isNew,
				(isNew) => {
					if (isNew !== undefined) {
						// Check if equal to match, if match not set it defaults to true.
						this.permitted = isNew === (this.config.match !== undefined ? this.config.match : true);
					}
				},
				ObserveSymbol,
			);
		});
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Entity Is New Condition',
	alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION,
	api: UmbWorkspaceEntityIsNewCondition,
};
