import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from '../input/entity-data-picker-data-source.context.token.js';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { isPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';

export class UmbEntityDataPickerSupportsTextFilterCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT, (context) => {
			this.observe(context?.dataSourceApi, (dataSourceApi) => {
				if (dataSourceApi && isPickerCollectionDataSource(dataSourceApi) && dataSourceApi.features) {
					this.observe(
						dataSourceApi.features.supportsTextFilter,
						(supportsTextFilter) => {
							this.permitted = supportsTextFilter.enabled;
						},
						'_observeSupportsTextFilter',
					);
				} else {
					this.removeUmbControllerByAlias('_observeSupportsTextFilter');
					this.permitted = false;
				}
			});
		});
	}
}
