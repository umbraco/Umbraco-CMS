import { UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT } from '../content-collection-workspace.context-token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbWorkspaceHasContentCollectionConditionConfig } from './types.js';

const ObserveSymbol = Symbol();

export class UmbWorkspaceHasContentCollectionCondition
	extends UmbConditionBase<UmbWorkspaceHasContentCollectionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbWorkspaceHasContentCollectionConditionConfig>,
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

export { UmbWorkspaceHasContentCollectionCondition as api };
