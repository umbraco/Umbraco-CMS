import { UmbConditionBase } from './condition-base.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbSwitchCondition extends UmbConditionBase<SwitchConditionConfig> implements UmbExtensionCondition {
	#timer?: ReturnType<typeof setTimeout>;

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<SwitchConditionConfig>) {
		super(host, args);
		this.startApprove();
	}

	startApprove() {
		clearTimeout(this.#timer);
		this.#timer = setTimeout(() => {
			this.permitted = true;
			this.startDisapprove();
		}, parseInt(this.config.frequency));
	}

	startDisapprove() {
		clearTimeout(this.#timer);
		this.#timer = setTimeout(() => {
			this.permitted = false;
			this.startApprove();
		}, parseInt(this.config.frequency));
	}

	override destroy() {
		clearTimeout(this.#timer);
		super.destroy();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Switch Condition',
	alias: 'Umb.Condition.Switch',
	api: UmbSwitchCondition,
};

export type SwitchConditionConfig = UmbConditionConfigBase<'Umb.Condition.Switch'> & {
	frequency: string;
};
