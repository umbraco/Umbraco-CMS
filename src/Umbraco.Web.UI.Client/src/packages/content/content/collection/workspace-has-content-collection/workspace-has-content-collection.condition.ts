import { UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT } from '../content-collection-workspace.context-token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_WORKSPACE_HAS_COLLECTION_CONDITION_ALIAS } from './constants.js';
import type { UmbWorkspaceHasCollectionConditionConfig } from './types.js';

const ObserveSymbol = Symbol();

export class UmbWorkspaceHasCollectionCondition
	extends UmbConditionBase<UmbWorkspaceHasCollectionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbWorkspaceHasCollectionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT, (context) => {
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

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Workspace Has Collection Condition',
	alias: UMB_WORKSPACE_HAS_COLLECTION_CONDITION_ALIAS,
	api: UmbWorkspaceHasCollectionCondition,
};
