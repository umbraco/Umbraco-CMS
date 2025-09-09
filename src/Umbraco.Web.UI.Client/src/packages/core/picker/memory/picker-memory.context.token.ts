import type { UmbPickerMemoryContext } from './picker-memory.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PICKER_MEMORY_CONTEXT = new UmbContextToken<UmbPickerMemoryContext>('UmbPickerMemoryContext');
