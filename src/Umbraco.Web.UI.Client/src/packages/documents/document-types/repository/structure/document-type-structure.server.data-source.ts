import type { UmbAllowedDocumentTypeModel } from './types.js';
import type { AllowedDocumentTypeModel } from '@umbraco-cms/backoffice/backend-api';
import { DocumentTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 *
 * @export
 * @class UmbDocumentTypeStructureServerDataSource
 * @extends {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbDocumentTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedDocumentTypeModel,
	UmbAllowedDocumentTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedAtRoot, getAllowedChildrenOf, mapper });
	}
}

// eslint-disable-next-line local-rules/no-direct-api-import
const getAllowedAtRoot = () => DocumentTypeResource.getDocumentTypeAllowedAtRoot({});

const getAllowedChildrenOf = (unique: string | null) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeResource.getDocumentTypeByIdAllowedChildren({ id: unique });
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeResource.getDocumentTypeAllowedAtRoot({});
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
