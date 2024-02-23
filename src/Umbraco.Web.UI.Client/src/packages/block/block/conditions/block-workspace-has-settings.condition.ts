import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbBlockWorkspaceHasSettingsCondition extends UmbBaseController implements UmbExtensionCondition {
	config: BlockWorkspaceHasSettingsConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<BlockWorkspaceHasSettingsConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.settings.contentTypeId,
				(settingsContentTypeId) => {
					this.permitted = !!settingsContentTypeId;
					this.#onChange();
				},
				'observeSettingsElementTypeId',
			);
		});
	}
}

export type BlockWorkspaceHasSettingsConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.BlockWorkspaceHasSettings'>;
