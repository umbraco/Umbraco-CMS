import { UmbConditionBase } from './condition-base.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbDelayCondition extends UmbConditionBase<DelayConditionConfig> implements UmbExtensionCondition {
	#timer?: ReturnType<typeof setTimeout>;

	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<DelayConditionConfig>) {
		super(host, args);
		this.#timer = setTimeout(() => {
			this.permitted = true;
		}, parseInt(this.config.offset));
	}

	override destroy() {
		clearTimeout(this.#timer);
		super.destroy();
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Delay Condition',
	alias: 'Umb.Condition.Delay',
	api: UmbDelayCondition,
};

// eslint-disable-next-line @typescript-eslint/naming-convention
export type DelayConditionConfig = UmbConditionConfigBase<'Umb.Condition.Delay'> & {
	offset: string;
};
