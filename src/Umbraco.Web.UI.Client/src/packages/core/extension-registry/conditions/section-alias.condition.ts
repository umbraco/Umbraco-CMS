import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';

export class UmbSectionAliasCondition extends UmbControllerBase implements UmbExtensionCondition {
	config: SectionAliasConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<SectionAliasConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_SECTION_CONTEXT, (context) => {
			this.observe(context.alias, (sectionAlias) => {
				this.permitted = sectionAlias === this.config.match;
				this.#onChange();
			});
		});
	}
}

export type SectionAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.SectionAlias'> & {
	/**
	 * Define the section that this extension should be available in
	 *
	 * @example "Umb.Section.Content"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Section Alias Condition',
	alias: 'Umb.Condition.SectionAlias',
	api: UmbSectionAliasCondition,
};
