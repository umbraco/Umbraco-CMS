import { UMB_BLOCK_CONTEXT } from '../context/block.context.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbBlockHasSettingsCondition extends UmbBaseController implements UmbExtensionCondition {
	config: BlockHasSettingsConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<BlockHasSettingsConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		console.log('blockHasSettings condition', this.getHostElement());

		this.consumeContext(UMB_BLOCK_CONTEXT, (context) => {
			console.log('UMB_BLOCK_CONTEXT!!!!!', context);
			this.observe(
				context.blockTypeSettingsElementTypeKey,
				(blockTypeSettingsElementTypeKey) => {
					console.log('condition', blockTypeSettingsElementTypeKey);
					this.permitted = !!blockTypeSettingsElementTypeKey;
					this.#onChange();
				},
				'observeSettingsElementTypeKey',
			);
		});
	}
}

export type BlockHasSettingsConditionConfig = UmbConditionConfigBase<'Umb.Condition.BlockHasSettings'>;

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Block Has Settings Condition',
	alias: 'Umb.Condition.BlockHasSettings',
	api: UmbBlockHasSettingsCondition,
};
