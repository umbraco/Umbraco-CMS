import type { ManifestElement } from "@umbraco-cms/backoffice/extension-api";

export enum PropertyValuePresentationDisplayOption {
  COLLECTION = 'collection'
}

export interface ManifestPropertyValuePresentation extends ManifestElement  {
    type: 'propertyValuePresentation';
    propertyEditorAlias: string;
  }

  declare global {
    interface UmbExtensionManifestMap {
      umbPropertyValuePresentation: ManifestPropertyValuePresentation;
    }
  }