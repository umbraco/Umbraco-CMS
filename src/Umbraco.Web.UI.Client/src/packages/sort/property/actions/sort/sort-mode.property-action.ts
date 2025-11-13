import { UMB_SORT_PROPERTY_CONTEXT } from '../../context/sort.property-context-token.js';
import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';

export class UmbSortModePropertyAction extends UmbPropertyActionBase {
	override async execute() {
		const sortContext = await this.getContext(UMB_SORT_PROPERTY_CONTEXT);
		sortContext?.toggle();
	}
}

export { UmbSortModePropertyAction as api };
