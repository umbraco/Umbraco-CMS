import type { UmbAllowedElementTypeModel } from './types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { AllowedDocumentTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbElementTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbElementTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedDocumentTypeModel,
	UmbAllowedElementTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
	}
}

const getAllowedChildrenOf = (_unique: string | null, _parentContentUnique: string | null) => {
	// eslint-disable-next-line local-rules/no-direct-api-import
	return DocumentTypeService.getDocumentTypeAllowedInLibrary({});
};

const mapper = (item: AllowedDocumentTypeModel): UmbAllowedElementTypeModel => {
	return {
		unique: item.id,
		entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};
