import { UmbPropertyActionBase } from '../../components/property-action/property-action-base.controller.js';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationDefaultData } from '@umbraco-cms/backoffice/notification';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

export class UmbCopyPropertyAction extends UmbPropertyActionBase {
	override async execute() {
		const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
		const value = propertyContext.getValue();

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		// TODO: Temporary solution to make something happen: [NL]
		const data: UmbNotificationDefaultData = { headline: 'Copied to clipboard', message: value };
		notificationContext?.peek('positive', { data });
	}
}
export default UmbCopyPropertyAction;
