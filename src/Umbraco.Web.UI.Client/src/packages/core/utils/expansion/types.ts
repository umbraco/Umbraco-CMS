import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export type UmbEntityExpansionModel<
	EntryModelType extends UmbEntityExpansionEntryModel = UmbEntityExpansionEntryModel,
> = Array<EntryModelType>;

export interface UmbEntityExpansionEntryModel extends UmbEntityModel {
	target?: UmbEntityModel;
}
