import type { UmbPickerContext } from './picker.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PICKER_CONTEXT = new UmbContextToken<UmbPickerContext>('UmbPickerContext');
