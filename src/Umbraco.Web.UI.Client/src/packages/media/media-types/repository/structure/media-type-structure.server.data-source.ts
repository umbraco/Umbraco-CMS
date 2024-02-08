import type { UmbAllowedMediaTypeModel } from './types.js';
import type { AllowedMediaTypeModel } from '@umbraco-cms/backoffice/backend-api';
import { MediaTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbContentTypeStructureServerDataSourceBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 *
 * @export
 * @class UmbMediaTypeStructureServerDataSource
 * @extends {UmbContentTypeStructureServerDataSourceBase}
 */
export class UmbMediaTypeStructureServerDataSource extends UmbContentTypeStructureServerDataSourceBase<
	AllowedMediaTypeModel,
	UmbAllowedMediaTypeModel
> {
	constructor(host: UmbControllerHost) {
		super(host, { getAllowedChildrenOf, mapper });
	}
}

const getAllowedChildrenOf = (unique: string | null) => {
	if (unique) {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeResource.getMediaTypeByIdAllowedChildren({ id: unique });
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return MediaTypeResource.getMediaTypeAllowedAtRoot({});
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
