import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ManifestCondition, UmbConditionConfig, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbSwitchCondition extends UmbBaseController implements UmbExtensionCondition {
	#timer?: any;
	config: UmbConditionConfig<string>;
	permitted = false;
	#onChange: () => void;

	constructor(args: { host: UmbControllerHost; config: UmbConditionConfig<string>; onChange: () => void }) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.startApprove();
	}

	startApprove() {
		this.#timer = setTimeout(() => {
			this.permitted = true;
			this.#onChange();
			this.startDisapprove();
		}, parseInt(this.config.value));
	}

	startDisapprove() {
		this.#timer = setTimeout(() => {
			this.permitted = false;
			this.#onChange();
			this.startApprove();
		}, parseInt(this.config.value));
	}

	destroy() {
		super.destroy();
		clearTimeout(this.#timer);
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Switch Condition',
	alias: 'Umb.Condition.Switch',
	class: UmbSwitchCondition,
};
