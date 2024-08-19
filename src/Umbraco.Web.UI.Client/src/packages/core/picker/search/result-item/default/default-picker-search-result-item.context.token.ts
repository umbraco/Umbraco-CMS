import type { UmbDefaultPickerSearchResultItemContext } from './default-picker-search-result-item.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PICKER_SEARCH_RESULT_ITEM_CONTEXT = new UmbContextToken<UmbDefaultPickerSearchResultItemContext>(
	'UmbPickerSearchResultItemContext',
);
