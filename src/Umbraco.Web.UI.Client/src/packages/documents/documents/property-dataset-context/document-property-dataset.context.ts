import type { UmbDocumentDetailModel, UmbDocumentVariantModel, UmbDocumentWorkspaceContext } from '../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbDocumentDetailModel,
	UmbDocumentTypeDetailModel,
	UmbDocumentVariantModel
> {
	constructor(host: UmbControllerHost, dataOwner: UmbDocumentWorkspaceContext, variantId?: UmbVariantId) {
		super(host, dataOwner, variantId);
	}
}
