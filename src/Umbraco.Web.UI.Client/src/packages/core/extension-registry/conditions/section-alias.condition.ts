import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ManifestCondition, UmbConditionConfig, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UMB_SECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';

export class UmbSectionAliasCondition extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfig<string>;
	permitted = false;
	#onChange: () => void;

	constructor(args: { host: UmbControllerHost; config: UmbConditionConfig<string>; onChange: () => void }) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (context) => {
			//if (context) {
			this.observe(
				context.alias,
				(sectionAlias) => {
					// TODO: Would be nice to make the object fully controllable by each condition, but requires some typing system.
					this.permitted = sectionAlias === this.config.value;
					this.#onChange();
				},
				'_observeSectionAlias'
			);
			//}
			// Niels: As is of this state, contexts cannot be unprovided, so this code is not needed:
			/*else {
				this.removeControllerByAlias('_observeSectionAlias');
				if (this.permitted === true) {
					this.permitted = false;
					this.#onChange();
				}
			}*/
		});
	}

	/*
	hostDisconnected() {
		super.hostDisconnected();
		this.permitted = false;
		this.#onChange();
	}
	*/
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Section Alias Condition',
	alias: 'Umb.Condition.SectionAlias',
	class: UmbSectionAliasCondition,
};
