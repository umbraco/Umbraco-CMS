import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export class RetrieveAction extends UmbEntityActionBase {

  async execute() {
    const { entityType, unique } = this.args;
    const message = `${entityType}_${unique}`;
    const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    notificationContext?.peek('positive', {
      data: {
        headline: '',
        message: message,
      },
    });
  }
}
export { RetrieveAction as api };
