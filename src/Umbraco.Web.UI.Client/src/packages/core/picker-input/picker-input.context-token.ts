import type { UmbPickerInputContext } from './picker-input.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PICKER_INPUT_CONTEXT = new UmbContextToken<UmbPickerInputContext>('UmbPickerInputContext');
