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

const getAllowedChildrenOf = (unique: string) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	DocumentTypeResource.getDocumentTypeByIdAllowedChildren({ id: unique });

const mapper = (item: AllowedDocumentTypeModel): UmbAllowedDocumentTypeModel => {
	return {
		unique: item.id,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};
