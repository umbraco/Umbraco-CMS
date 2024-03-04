import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbBlockWorkspaceHasSettingsCondition
	extends UmbConditionBase<BlockWorkspaceHasSettingsConditionConfig>
	implements UmbExtensionCondition
{
	constructor(args: UmbConditionControllerArguments<BlockWorkspaceHasSettingsConditionConfig>) {
		super(args);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.settings.contentTypeId,
				(settingsContentTypeId) => {
					this.permitted = !!settingsContentTypeId;
				},
				'observeSettingsElementTypeId',
			);
		});
	}
}

export default UmbBlockWorkspaceHasSettingsCondition;

export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;
