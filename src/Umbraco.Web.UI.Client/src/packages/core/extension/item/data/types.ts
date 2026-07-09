import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import type { UmbWithDescriptionModel } from '@umbraco-cms/backoffice/models';

export interface UmbExtensionItemModel extends UmbItemModel, UmbWithDescriptionModel {
	manifest: ManifestBase;
}
