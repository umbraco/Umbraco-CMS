import { UMB_PROPERTY_SORT_MODE_CONTEXT } from '../context/property-sort-mode.context-token.js';
import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';

export class UmbPropertySortModePropertyAction extends UmbPropertyActionBase {
	override async execute() {
		const sortContext = await this.getContext(UMB_PROPERTY_SORT_MODE_CONTEXT);
		sortContext?.toggleSortingMode();
	}
}

export { UmbPropertySortModePropertyAction as api };
