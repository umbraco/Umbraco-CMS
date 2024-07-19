import type { UmbDocumentVariantModel } from '../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';

export class UmbDocumentPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbDocumentTypeDetailModel,
	UmbDocumentVariantModel
> {}
