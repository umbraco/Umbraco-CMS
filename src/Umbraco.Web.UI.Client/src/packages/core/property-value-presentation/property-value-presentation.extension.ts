import type { ManifestElement } from "@umbraco-cms/backoffice/extension-api";

export enum PropertyValuePresentationDisplayOption {
  COLLECTION_COLUMN = 'collection-column',
  COLLECTION_CARD = 'collection-card',
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