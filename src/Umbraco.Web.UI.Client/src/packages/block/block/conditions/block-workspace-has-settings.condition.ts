import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
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

		// TODO: Rename the Block Context, so it gets a name.. like Block Entry Context or something.
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

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Block Has Settings Condition',
	alias: 'Umb.Condition.BlockWorkspaceHasSettings',
	api: UmbBlockWorkspaceHasSettingsCondition,
};
