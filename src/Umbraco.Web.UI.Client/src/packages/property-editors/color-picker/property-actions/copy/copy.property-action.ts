import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationDefaultData } from '@umbraco-cms/backoffice/notification';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';

export class UmbCopyColorPickerPropertyAction extends UmbPropertyActionBase {
	override async execute() {
		alert('CopyColorPickerPropertyAction');
	}
}
export { UmbCopyColorPickerPropertyAction as api };
