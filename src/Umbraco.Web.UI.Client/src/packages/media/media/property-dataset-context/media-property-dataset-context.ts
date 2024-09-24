import type { UmbMediaDetailModel, UmbMediaVariantModel } from '../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbMediaTypeDetailModel } from '@umbraco-cms/backoffice/media-type';

export class UmbMediaPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbMediaDetailModel,
	UmbMediaTypeDetailModel,
	UmbMediaVariantModel
> {}
