import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbSearchIndexProviderNameConditionConfig extends UmbConditionConfigBase {
  /**
   * Define the provider name that this extension should be available for
   * @example
   * "Examine"
   */
  match?: string;
  /**
   * Define one or more provider names that this extension should be available for
   * @example
   * ["Examine"]
   */
  oneOf?: Array<string>;
}

declare global {
  interface UmbExtensionConditionConfigMap {
    umbSearchIndexProviderName: UmbSearchIndexProviderNameConditionConfig;
  }
}
