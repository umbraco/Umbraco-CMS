import { UMB_MODAL_CONTEXT } from '../../context/modal.context-token.js';
import type { UmbInModalConditionConfig } from './in-modal.condition-config.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbInModalCondition extends UmbConditionBase<UmbInModalConditionConfig> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbInModalConditionConfig>) {
		super(host, args);

		// Default match value is true (checking if we are in a modal)
		const matchValue = this.config.match ?? true;

		// Default state: we assume we are NOT in a modal.
		// So if match is false (looking for "not in modal"), we start as permitted.
		// If match is true (looking for "in modal"), we start as not permitted.
		this.permitted = !matchValue;

		this.consumeContext(UMB_MODAL_CONTEXT, () => {
			// Modal context found, so we ARE in a modal.
			// Permitted if match is true, not permitted if match is false.
			this.permitted = matchValue;
		});
	}
}

export { UmbInModalCondition as api };
