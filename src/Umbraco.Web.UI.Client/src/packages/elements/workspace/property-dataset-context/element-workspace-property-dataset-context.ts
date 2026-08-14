import type { UmbElementDetailModel, UmbElementVariantModel } from '../../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';

export class UmbElementWorkspacePropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbElementDetailModel,
	UmbDocumentTypeDetailModel,
	UmbElementVariantModel
> {}
