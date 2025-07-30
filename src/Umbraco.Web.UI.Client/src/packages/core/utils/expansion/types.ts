import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type UmbEntityExpansionModel = Array<UmbEntityModel>;

export interface UmbEntityExpansionEntryModel extends UmbEntityModel {
	target?: UmbEntityModel;
}
