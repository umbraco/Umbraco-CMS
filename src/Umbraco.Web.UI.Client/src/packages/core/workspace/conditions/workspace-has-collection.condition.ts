import { UMB_COLLECTION_WORKSPACE_CONTEXT } from '../contexts/tokens/collection-workspace.context-token.js';
import type { WorkspaceHasCollectionConditionConfig } from './types.js';
import { UMB_WORKSPACE_HAS_COLLECTION_CONDITION } from './const.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbWorkspaceHasCollectionCondition
	extends UmbConditionBase<WorkspaceHasCollectionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceHasCollectionConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_COLLECTION_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.contentTypeHasCollection,
				(hasCollection) => {
					this.permitted = hasCollection;
				},
				ObserveSymbol,
			);
		});
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Has Collection Condition',
	alias: UMB_WORKSPACE_HAS_COLLECTION_CONDITION,
	api: UmbWorkspaceHasCollectionCondition,
};
