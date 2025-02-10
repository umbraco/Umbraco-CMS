import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbServerResponseModel {
	$type: string;
}

export interface UmbDataMapper<
	ServerModelType extends UmbServerResponseModel = UmbServerResponseModel,
	ClientModelType extends UmbEntityModel = UmbEntityModel,
> extends UmbApi {
	map: (data: ServerModelType) => Promise<ClientModelType>;
}
