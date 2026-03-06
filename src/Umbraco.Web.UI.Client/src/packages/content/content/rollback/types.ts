import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export interface UmbContentRollbackVersionItemModel {
	id: string;
	versionDate: string;
	user: { id: string };
	isCurrentDraftVersion: boolean;
	isCurrentPublishedVersion: boolean;
	preventCleanup: boolean;
}

export interface UmbContentRollbackVersionDetailModel {
	id: string;
	variants: Array<Pick<UmbEntityVariantModel, 'culture' | 'name'>>;
	values: Array<{ culture: string | null; alias: string; value: unknown }>;
}
