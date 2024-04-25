import type {
	UmbContentTypeCompositionCompatibleModel,
	UmbContentTypeCompositionReferenceModel,
	UmbContentTypeCompositionRequestModel,
} from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeCompositionDataSourceConstructor {
	new (host: UmbControllerHost): UmbContentTypeCompositionDataSource;
}

export interface UmbContentTypeCompositionDataSource {
	availableCompositions<
		ResponseType extends UmbContentTypeCompositionCompatibleModel,
		ArgsType extends UmbContentTypeCompositionRequestModel,
	>(
		args: ArgsType,
	): Promise<UmbDataSourceResponse<Array<ResponseType>>>;
	getReferences<ResponseType extends UmbContentTypeCompositionReferenceModel>(
		unique: string,
	): Promise<UmbDataSourceResponse<Array<ResponseType>>>;
}
