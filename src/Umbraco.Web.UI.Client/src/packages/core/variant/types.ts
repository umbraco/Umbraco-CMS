import type { ContentStateModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbVariantModel {
	createDate: string | null;
	culture: string | null;
	name: string;
	publishDate: string | null;
	segment: string | null;
	state: ContentStateModel | null;
	updateDate: string | null;
}
