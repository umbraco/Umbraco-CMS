import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';
import { UMB_SORT_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/sorter';

export class UmbSortModePropertyAction extends UmbPropertyActionBase {
	override async execute() {
		const sortContext = await this.getContext(UMB_SORT_PROPERTY_CONTEXT);
		sortContext?.toggleSortingMode();
	}
}

export { UmbSortModePropertyAction as api };
