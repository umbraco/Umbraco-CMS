import type { UmbAllowedDocumentTypeModel } from './types.js';
import type { AllowedDocumentTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 *
 
 * @class UmbDocumentTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbDocumentTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedDocumentTypeModel,
	UmbAllowedDocumentTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
	}
}

const getAllowedChildrenOf = (unique: string | null) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeService.getDocumentTypeByIdAllowedChildren({ id: unique });
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeService.getDocumentTypeAllowedAtRoot({});
	}
};

const mapper = (item: AllowedDocumentTypeModel): UmbAllowedDocumentTypeModel => {
	return {
		unique: item.id,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};
