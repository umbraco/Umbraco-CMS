import { UmbPropertyActionBase } from '@umbraco-cms/backoffice/property-action';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
export class WritePropertyAction extends UmbPropertyActionBase {
  async execute() {
    const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
    if (!propertyContext) {
      return;
    }
    propertyContext.setValue('Hello world');
    
  }
}
export { WritePropertyAction as api };
