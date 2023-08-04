import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbSwitchCondition extends UmbBaseController implements UmbExtensionCondition {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	#timer?: any;
	config: SwitchConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: { host: UmbControllerHost; config: SwitchConditionConfig; onChange: () => void }) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.startApprove();
	}

	startApprove() {
		clearTimeout(this.#timer);
		this.#timer = setTimeout(() => {
			this.permitted = true;
			this.#onChange();
			this.startDisapprove();
		}, parseInt(this.config.frequency));
	}

	startDisapprove() {
		clearTimeout(this.#timer);
		this.#timer = setTimeout(() => {
			this.permitted = false;
			this.#onChange();
			this.startApprove();
		}, parseInt(this.config.frequency));
	}

	destroy() {
		clearTimeout(this.#timer);
		super.destroy();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Switch Condition',
	alias: 'Umb.Condition.Switch',
	class: UmbSwitchCondition,
};

export type SwitchConditionConfig = UmbConditionConfigBase & {
	frequency: string;
};
