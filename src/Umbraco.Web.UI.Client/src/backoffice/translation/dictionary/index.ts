import { DictionaryItemTranslationModel, EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface DictionaryDetails extends EntityTreeItemResponseModel {
	key: string; // TODO: Remove this when the backend is fixed
	translations: DictionaryItemTranslationModel[];
}
