import { UmbPropertyActionBase } from '../../property-action-base.js';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

export class UmbClearPropertyAction extends UmbPropertyActionBase {
	override async execute() {
		const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
		propertyContext.clearValue();
	}
}
export default UmbClearPropertyAction;
