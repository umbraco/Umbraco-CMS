import type { UmbContentPickerSource } from '../types.js';
import type { UmbContentPickerSourceTypeConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

/**
 * Permits an extension only where the surrounding Content Picker is configured for a given source type.
 *
 * A Content Picker serves documents, media and members through one property editor UI alias, so extensions that
 * only make sense for one of them cannot be selected by alias alone.
 */
export class UmbContentPickerSourceTypeCondition
	extends UmbConditionBase<UmbContentPickerSourceTypeConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbContentPickerSourceTypeConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context?.configValues,
				(configValues) => {
					const startNode = configValues?.find((property) => property.alias === 'startNode')?.value as
						| UmbContentPickerSource
						| undefined;

					this.permitted = startNode?.type === this.config.match;
				},
				'observeContentPickerSourceType',
			);
		});
	}
}

export { UmbContentPickerSourceTypeCondition as api };
