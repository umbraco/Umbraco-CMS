import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_SECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';

export class UmbSectionAliasCondition extends UmbBaseController implements UmbExtensionCondition {
	config: SectionAliasConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<SectionAliasConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (context) => {
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
	 * @examples
	 * "Umb.Section.Content"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Section Alias Condition',
	alias: 'Umb.Condition.SectionAlias',
	class: UmbSectionAliasCondition,
};
