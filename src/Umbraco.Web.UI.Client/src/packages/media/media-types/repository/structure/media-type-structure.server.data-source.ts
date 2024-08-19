import type { UmbAllowedMediaTypeModel } from './types.js';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { AllowedMediaTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 
 * @class UmbMediaTypeStructureServerDataSource
 * @augments {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbMediaTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedMediaTypeModel,
	UmbAllowedMediaTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
	}

	getMediaTypesOfFileExtension({ fileExtension, skip, take }: { fileExtension: string; skip: number; take: number }) {
		return getAllowedMediaTypesOfExtension({ fileExtension, skip, take });
	}
}

const getAllowedChildrenOf = (unique: string | null) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeService.getMediaTypeByIdAllowedChildren({ id: unique });
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeService.getMediaTypeAllowedAtRoot({});
	}
};

const mapper = (item: AllowedMediaTypeModel): UmbAllowedMediaTypeModel => {
	return {
		unique: item.id,
		name: item.name,
		description: item.description || null,
		icon: item.icon || null,
	};
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
	const { items } = await MediaTypeService.getItemMediaTypeAllowed({ fileExtension, skip, take });
	return items.map((item) => mapper(item));
};
