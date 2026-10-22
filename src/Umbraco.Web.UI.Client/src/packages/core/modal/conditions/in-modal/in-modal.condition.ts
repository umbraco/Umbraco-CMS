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

		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this.permitted = !!context === matchValue;
		})
			.asPromise()
			.catch(() => {
				// The context could not be found, so it is absent: permitted if that matches the expected match value.
				this.permitted = !matchValue;
			});
	}
}

export { UmbInModalCondition as api };
