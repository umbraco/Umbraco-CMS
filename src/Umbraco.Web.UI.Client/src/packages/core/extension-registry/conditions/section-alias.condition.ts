import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ManifestCondition, UmbConditionConfig, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UMB_SECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';

export class UmbSectionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: UmbConditionConfig<string>;
	permitted = false;
	#onChange: () => void;

	constructor(args: { host: UmbControllerHost; config: UmbConditionConfig<string>; onChange: () => void }) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_SECTION_CONTEXT_TOKEN, (context) => {
			if (context) {
				console.log('got new context', context, this.config.value);
				this.observe(
					context.alias,
					(sectionAlias) => {
						// TODO: Would be nice to change to match:
						console.log(sectionAlias, ' === ', this.config.value, sectionAlias === this.config.value);
						this.permitted = sectionAlias === this.config.value;
						this.#onChange();
					},
					'_observeSectionAlias'
				);
			} else {
				this.permitted = false;
				this.#onChange();
				this.removeControllerByAlias('_observeSectionAlias');
			}
		});
	}

	/*
	hostDisconnected() {
		super.hostDisconnected();
		console.log('Condition disconnected');
		this.permitted = false;
		this.#onChange();
	}
	*/
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Section Alias Condition',
	alias: 'Umb.Condition.SectionAlias',
	class: UmbSectionCondition,
};
