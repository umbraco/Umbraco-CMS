import type { UmbEntityDataPickerDataSourceApiContext } from './entity-data-picker-data-source.context.js';
import type { UmbPickerPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

const contextAlias = 'UmbEntityDataPickerDataSourceApiContext';

export const UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT = new UmbContextToken<
	UmbEntityDataPickerDataSourceApiContext<UmbPickerPropertyEditorDataSource>
>(contextAlias);
