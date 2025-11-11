import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
export class ReadPropertyAction extends UmbPropertyActionBase {
  async execute() {
    const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
    if (!propertyContext) {
      return;
    }
    const value = propertyContext.getValue();
    const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    notificationContext?.peek('positive', {
      data: {
        headline: '',
        message: value,
      },
    });
  }
}
export { ReadPropertyAction as api };
