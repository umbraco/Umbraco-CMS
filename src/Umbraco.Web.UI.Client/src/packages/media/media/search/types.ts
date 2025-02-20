import type { UmbMediaItemModel } from '../types.js';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbMediaTypeEntityType } from '@umbraco-cms/backoffice/media-type';

export interface UmbMediaSearchItemModel extends UmbMediaItemModel {
	href: string;
}

export interface UmbMediaSearchRequestArgs extends UmbSearchRequestArgs {
	allowedContentTypes?: Array<{ unique: string; entityType: UmbMediaTypeEntityType }>;
}
