import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbAllowedMediaTypeModel, UmbMediaTypeStructureDataSource } from './types.js';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { AllowedMediaTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

/**
 * @class UmbMediaTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbMediaTypeStructureServerDataSource
	extends UmbContentTypeStructureServerDataSourceBase<AllowedMediaTypeModel, UmbAllowedMediaTypeModel>
	implements UmbMediaTypeStructureDataSource
{
	#host: UmbControllerHost;
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
		this.#host = host;
	}

	getMediaTypesOfFileExtension({ fileExtension, skip, take }: { fileExtension: string; skip: number; take: number }) {
		return getAllowedMediaTypesOfExtension(this.#host, { fileExtension, skip, take });
	}

	getMediaTypesOfFolders({ skip, take }: { skip: number; take: number }) {
		return getAllowedMediaTypesOfFolders(this.#host, { skip, take });
	}

	async getAllowedParentsOf(unique: string) {
		const { data, error } = await tryExecute(
			this.#host,
			MediaTypeService.getMediaTypeByIdAllowedParents({
				path: { id: unique },
			}),
		);
		if (error) {
			return { error };
		}
		const mappedData =
			data?.allowedParentIds.map((item) => ({
				unique: item.id,
				entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			})) ?? [];
		return { data: mappedData };
	}
}

const getAllowedChildrenOf = (
	unique: string | null,
	parentContentUnique: string | null,
	paging?: UmbOffsetPaginationRequestModel,
) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeService.getMediaTypeByIdAllowedChildren({
			path: { id: unique },
			query: { parentContentKey: parentContentUnique ?? undefined, skip: paging?.skip, take: paging?.take },
		});
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeService.getMediaTypeAllowedAtRoot({
			query: { skip: paging?.skip, take: paging?.take },
		});
	}
};

const mapper = (item: AllowedMediaTypeModel): UmbAllowedMediaTypeModel => {
	return {
		unique: item.id,
		entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
};

const getAllowedMediaTypesOfFolders = async (
	host: UmbControllerHost,
	{ skip, take }: { skip: number; take: number },
): Promise<UmbDataSourceResponse<UmbPagedModel<UmbAllowedMediaTypeModel>>> => {
	const { data, error } = await tryExecute(
		host,
		// eslint-disable-next-line local-rules/no-direct-api-import
		MediaTypeService.getItemMediaTypeFolders({ query: { skip, take } }),
	);

	if (!data) {
		return { error };
	}

	return { data: { items: data.items.map((item) => mapper(item)), total: data.total } };
};

const getAllowedMediaTypesOfExtension = async (
	host: UmbControllerHost,
	{ fileExtension, skip, take }: { fileExtension: string; skip: number; take: number },
): Promise<UmbDataSourceResponse<UmbPagedModel<UmbAllowedMediaTypeModel>>> => {
	const { data, error } = await tryExecute(
		host,
		// eslint-disable-next-line local-rules/no-direct-api-import
		MediaTypeService.getItemMediaTypeAllowed({ query: { fileExtension, skip, take } }),
	);

	if (!data) {
		return { error };
	}

	return {
		data: {
			items: data.items.map((item) => ({ ...mapper(item), matchedFileExtension: item.matchedFileExtension })),
			total: data.total,
		},
	};
};
