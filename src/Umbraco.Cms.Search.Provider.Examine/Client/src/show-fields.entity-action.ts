import { fieldsRouteBuilder } from './fields-route-provider.element.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_SEARCH_WORKSPACE_CONTEXT } from '@umbraco-cms/search/settings';

export class UmbSearchExamineShowFieldsEntityAction extends UmbEntityActionBase<never> {
  override async getHref() {
    const unique = this.args.unique ?? null;
    if (!unique) return '#';

    const workspaceContext = await this.getContext(UMB_SEARCH_WORKSPACE_CONTEXT);
    const culture = workspaceContext?.getSelectedCulture() ?? 'none';

    return fieldsRouteBuilder?.({ documentUnique: unique, culture }) ?? '#';
  }

  override execute(): Promise<void> {
    return Promise.resolve(undefined);
  }
}

export default UmbSearchExamineShowFieldsEntityAction;
