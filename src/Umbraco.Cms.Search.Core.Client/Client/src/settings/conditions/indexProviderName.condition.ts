import {
  ManifestCondition,
  UmbConditionControllerArguments,
  UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import {
  UMB_SEARCH_WORKSPACE_CONTEXT,
  UmbSearchIndexProviderNameConditionConfig,
} from '@umbraco-cms/search/settings';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { stringOrStringArrayContains } from '@umbraco-cms/backoffice/utils';

export class UmbSearchIndexProviderNameCondition
  extends UmbConditionBase<UmbSearchIndexProviderNameConditionConfig>
  implements UmbExtensionCondition
{
  constructor(
    host: UmbControllerHost,
    args: UmbConditionControllerArguments<UmbSearchIndexProviderNameConditionConfig>,
  ) {
    super(host, args);

    const matchArray =
      args.config.oneOf ?? (args.config.match ? [args.config.match] : undefined) ?? [];

    this.consumeContext(UMB_SEARCH_WORKSPACE_CONTEXT, (context) => {
      this.observe(
        context?.providerName,
        (providerName) => {
          this.permitted = providerName
            ? stringOrStringArrayContains(matchArray, providerName)
            : false;
        },
        '_observeProviderName',
      );
    });
  }
}

export const manifest: ManifestCondition = {
  type: 'condition',
  name: 'Search Index Provider Name Condition',
  alias: 'Umb.Search.Condition.IndexProviderName',
  api: UmbSearchIndexProviderNameCondition,
};
