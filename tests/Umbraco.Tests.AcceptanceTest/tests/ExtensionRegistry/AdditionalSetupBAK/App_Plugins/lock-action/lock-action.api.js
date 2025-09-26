import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbDocumentLockEntityAction extends UmbEntityActionBase {
  async execute() {
    const context = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT);
    const ruleUnique = 'lock';

    const myRule = {
      unique: ruleUnique,
      UmbVariantId: new UmbVariantId(),
    };

    const hasRule = context?.readOnlyGuard.getRules().find((rule) => rule.unique === ruleUnique);

    if (hasRule) {
      context?.readOnlyGuard.removeRule(ruleUnique);
    } else {
      context?.readOnlyGuard.addRule(myRule);
    }
  }
}

export { UmbDocumentLockEntityAction as api };