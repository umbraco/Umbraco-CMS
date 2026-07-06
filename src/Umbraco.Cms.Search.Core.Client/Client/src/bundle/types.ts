import type {
  ManifestElement,
  ManifestWithDynamicConditions,
} from '@umbraco-cms/backoffice/extension-api';

export interface ManifestSearchIndexDetailBox
  extends ManifestElement, ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
  type: 'searchIndexDetailBox';
  meta?: MetaSearchIndexDetailBox;
}

export interface MetaSearchIndexDetailBox {
  label?: string;
  column?: 'left' | 'right';
}

declare global {
  interface UmbExtensionManifestMap {
    umbSearchIndexDetailBox: ManifestSearchIndexDetailBox;
  }
}
