import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbAllowedMediaTypeModel, UmbMediaTypeStructureDataSource } from './types.js';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { AllowedMediaTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

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
		return getAllowedMediaTypesOfExtension({ fileExtension, skip, take });
	}

	getMediaTypesOfFolders({ skip, take }: { skip: number; take: number }) {
		return getAllowedMediaTypesOfFolders({ skip, take });
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

const getAllowedChildrenOf = (unique: string | null, parentContentUnique: string | null) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeService.getMediaTypeByIdAllowedChildren({
			path: { id: unique },
			query: { parentContentKey: parentContentUnique ?? undefined },
		});
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeService.getMediaTypeAllowedAtRoot({});
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

const getAllowedMediaTypesOfFolders = async ({ skip, take }: { skip: number; take: number }) => {
	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data } = await MediaTypeService.getItemMediaTypeFolders({ query: { skip, take } });
	return data.items.map((item) => mapper(item));
};

const getAllowedMediaTypesOfExtension = async ({
	fileExtension,
	skip,
	take,
}: {
	fileExtension: string;
	skip: number;
	take: number;
}) => {
	// eslint-disable-next-line local-rules/no-direct-api-import
	const { data } = await MediaTypeService.getItemMediaTypeAllowed({ query: { fileExtension, skip, take } });
	return data.items.map((item) => mapper(item));
};
