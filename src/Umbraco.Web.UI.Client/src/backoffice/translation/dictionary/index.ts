import { DictionaryItemTranslationModel, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: Can we get rid of this type? I guess it should come from the server? Investigate this.
export interface DictionaryDetails extends EntityTreeItemResponseModel {
	translations: DictionaryItemTranslationModel[];
}
