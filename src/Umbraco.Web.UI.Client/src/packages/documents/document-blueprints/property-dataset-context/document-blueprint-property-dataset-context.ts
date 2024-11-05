import type { UmbDocumentBlueprintDetailModel, UmbDocumentBlueprintVariantModel } from '../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';

export class UmbDocumentBlueprintPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbDocumentBlueprintDetailModel,
	UmbDocumentTypeDetailModel,
	UmbDocumentBlueprintVariantModel
> {}
