import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbEntityDetailWorkspaceContextArgs,
} from '@umbraco-cms/backoffice/workspace';

export interface UmbContentTypeWorkspaceContextArgs extends UmbEntityDetailWorkspaceContextArgs {}

export abstract class UmbContentTypeWorkspaceContextBase<
	DetailModelType extends UmbEntityModel,
	DetailRepositoryType extends UmbDetailRepository<DetailModelType> = UmbDetailRepository<DetailModelType>,
> extends UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType> {
	constructor(host: UmbControllerHost, args: UmbContentTypeWorkspaceContextArgs) {
		super(host, args);
	}
}
