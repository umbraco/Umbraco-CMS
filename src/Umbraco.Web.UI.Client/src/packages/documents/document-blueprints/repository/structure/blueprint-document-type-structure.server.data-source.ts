import type { UmbAllowedBlueprintDocumentTypeModel } from './types.js';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import type { AllowedDocumentTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbBlueprintDocumentTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbBlueprintDocumentTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedDocumentTypeModel,
	UmbAllowedBlueprintDocumentTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
	}
}

const getAllowedChildrenOf = (_unique: string | null, parentUnique: string | null) => {
	// eslint-disable-next-line local-rules/no-direct-api-import
	return DocumentTypeService.getDocumentTypeAllowedForBlueprint({
		query: { parentKey: parentUnique ?? undefined },
	});
};

const mapper = (item: AllowedDocumentTypeModel): UmbAllowedBlueprintDocumentTypeModel => {
	return {
		unique: item.id,
		entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};
