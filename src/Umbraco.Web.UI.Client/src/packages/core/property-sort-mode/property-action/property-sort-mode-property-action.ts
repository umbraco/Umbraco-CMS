import { UMB_PROPERTY_SORT_MODE_CONTEXT } from '../property-context/property-sort-mode.context-token.js';
import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';

export class UmbPropertySortModePropertyAction extends UmbPropertyActionBase {
	override async execute() {
		const context = await this.getContext(UMB_PROPERTY_SORT_MODE_CONTEXT);
		context?.toggleIsSortMode();
	}
}

export { UmbPropertySortModePropertyAction as api };
