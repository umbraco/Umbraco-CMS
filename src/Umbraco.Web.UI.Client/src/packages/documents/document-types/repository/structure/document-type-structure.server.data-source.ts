import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbAllowedDocumentTypeModel, UmbDocumentTypeStructureDataSource } from './types.js';
import type { AllowedDocumentTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 *
 
 * @class UmbDocumentTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbDocumentTypeStructureServerDataSource
	extends UmbContentTypeStructureServerDataSourceBase<AllowedDocumentTypeModel, UmbAllowedDocumentTypeModel>
	implements UmbDocumentTypeStructureDataSource
{
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
		this.#host = host;
	}

	async getAllowedParentsOf(unique: string): Promise<UmbDataSourceResponse<Array<UmbEntityModel>>> {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentTypeService.getDocumentTypeByIdAllowedParents({
				path: { id: unique },
			}),
		);
		if (error) {
			return { error };
		}
		const mappedData =
			data?.allowedParentIds.map((item) => ({
				unique: item.id,
				entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			})) ?? [];
		return { data: mappedData };
	}
}

const getAllowedChildrenOf = (unique: string | null, parentContentUnique: string | null) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeService.getDocumentTypeByIdAllowedChildren({
			path: { id: unique },
			query: { parentContentKey: parentContentUnique ?? undefined },
		});
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return DocumentTypeService.getDocumentTypeAllowedAtRoot({});
	}
};

const mapper = (item: AllowedDocumentTypeModel): UmbAllowedDocumentTypeModel => {
	return {
		unique: item.id,
		entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};
