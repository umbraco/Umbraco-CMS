import type { UmbPickerModalContext } from './picker-modal.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PICKER_MODAL_CONTEXT = new UmbContextToken<UmbPickerModalContext>('UmbPickerModalContext');
