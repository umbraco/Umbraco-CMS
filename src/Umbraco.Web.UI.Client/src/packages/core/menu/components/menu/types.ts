import type { UmbEntityExpansionEntryModel } from '@umbraco-cms/backoffice/utils';

export interface UmbMenuItemExpansionEntryModel extends UmbEntityExpansionEntryModel {
	menuItemAlias: string;
}
