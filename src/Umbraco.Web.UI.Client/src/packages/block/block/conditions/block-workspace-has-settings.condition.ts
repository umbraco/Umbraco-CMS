import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import type { BlockWorkspaceHasSettingsConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockWorkspaceHasSettingsCondition
	extends UmbConditionBase<BlockWorkspaceHasSettingsConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<BlockWorkspaceHasSettingsConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.settings.contentTypeId,
				(settingsContentTypeId) => {
					this.permitted = settingsContentTypeId !== undefined;
				},
				'observeSettingsElementTypeId',
			);
		});
	}
}

export default UmbBlockWorkspaceHasSettingsCondition;
