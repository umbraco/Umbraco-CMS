import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbConditionConfigBase, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

export class UmbConditionBase<ConditionConfigType extends UmbConditionConfigBase>
	extends UmbControllerBase
	implements UmbExtensionCondition
{
	public readonly config: ConditionConfigType;
	#permitted?: boolean;
	public get permitted(): boolean | undefined {
		return this.#permitted;
	}
	public set permitted(value: boolean) {
		if (value === this.#permitted) return;
		this.#permitted = value;
		this.#onChange?.(value);
	}
	#onChange: ((permitted: boolean) => void) | undefined;

	constructor(host: UmbControllerHost, args: { config: ConditionConfigType; onChange: (permitted: boolean) => void }) {
		super(host);
		this.config = args.config;
		this.#onChange = args.onChange;
	}

	override destroy() {
		// Clear `#onChange` before `super.destroy()`. [NL]
		this.#onChange = undefined;
		super.destroy();
		(this.config as unknown) = undefined;
	}
}
