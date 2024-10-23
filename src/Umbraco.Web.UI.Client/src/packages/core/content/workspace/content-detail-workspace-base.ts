import type { UmbContentDetailModel } from '../types.js';
import type { UmbContentWorkspaceContext } from './content-workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import {
	UmbEntityDetailWorkspaceContextBase,
	type UmbEntityDetailWorkspaceContextArgs,
	type UmbEntityDetailWorkspaceContextCreateArgs,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbContentTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export abstract class UmbContentDetailWorkspaceBase<
		DetailModelType extends UmbContentDetailModel,
		DetailRepositoryType extends UmbDetailRepository<DetailModelType> = UmbDetailRepository<DetailModelType>,
		ContentTypeDetailModel extends UmbContentTypeModel = UmbContentTypeModel,
		VariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
		CreateArgsType extends
			UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType> = UmbEntityDetailWorkspaceContextCreateArgs<DetailModelType>,
	>
	extends UmbEntityDetailWorkspaceContextBase<DetailModelType, DetailRepositoryType, CreateArgsType>
	implements UmbContentWorkspaceContext<DetailModelType, ContentTypeDetailModel, VariantModelType>
{
	constructor(host: UmbControllerHost, args: UmbEntityDetailWorkspaceContextArgs) {
		super(host, args);
	}
}
