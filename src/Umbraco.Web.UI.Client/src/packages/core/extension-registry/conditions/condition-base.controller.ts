import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbConditionConfigBase, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbConditionBase<ConditionConfigType extends UmbConditionConfigBase>
	extends UmbControllerBase
	implements UmbExtensionCondition
{
	public readonly config: ConditionConfigType;
	#permitted = false;
	public get permitted() {
		return this.#permitted;
	}
	public set permitted(value) {
		if (value === this.#permitted) return;
		this.#permitted = value;
		this.#onChange();
	}
	#onChange: () => void;

	constructor(host: UmbControllerHost, args: { config: ConditionConfigType; onChange: () => void }) {
		super(host);
		this.config = args.config;
		this.#onChange = args.onChange;
	}

	override destroy() {
		super.destroy();
		(this.config as unknown) = undefined;
		(this.#onChange as unknown) = undefined;
	}
}
