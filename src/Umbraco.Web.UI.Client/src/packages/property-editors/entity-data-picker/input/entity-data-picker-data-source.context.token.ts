import type { UmbEntityDataPickerDataSourceApiContext } from './entity-data-picker-data-source.context.js';
import type { UmbPickerDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

const contextAlias = 'UmbEntityDataPickerDataSourceApiContext';

export const UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT = new UmbContextToken<
	UmbEntityDataPickerDataSourceApiContext<UmbPickerDataSource>
>(contextAlias);
