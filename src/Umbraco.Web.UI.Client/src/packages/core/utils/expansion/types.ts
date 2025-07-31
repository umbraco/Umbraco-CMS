import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type UmbEntityExpansionModel = Array<UmbEntityExpansionEntryModel>;

export interface UmbEntityExpansionEntryModel extends UmbEntityModel {
	target?: UmbEntityModel;
}
