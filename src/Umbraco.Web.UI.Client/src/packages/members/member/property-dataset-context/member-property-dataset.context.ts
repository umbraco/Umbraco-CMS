import type { UmbMemberDetailModel, UmbMemberVariantModel } from '../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbMemberTypeDetailModel } from '@umbraco-cms/backoffice/member-type';

export class UmbMemberPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbMemberDetailModel,
	UmbMemberTypeDetailModel,
	UmbMemberVariantModel
> {}
