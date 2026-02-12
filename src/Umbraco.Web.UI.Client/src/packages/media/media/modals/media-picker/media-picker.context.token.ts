import type { UmbMediaPickerContext } from './media-picker.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEDIA_PICKER_CONTEXT = new UmbContextToken<UmbMediaPickerContext>(
	'UmbPickerContext',
	undefined,
	(context): context is UmbMediaPickerContext => context.IS_MEDIA_PICKER_CONTEXT,
);
