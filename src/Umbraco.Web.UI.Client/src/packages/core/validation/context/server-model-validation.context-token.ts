import type { UmbServerModelValidationContext } from './index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SERVER_MODEL_VALIDATION_CONTEXT = new UmbContextToken<UmbServerModelValidationContext>(
	'UmbServerModelValidationContext',
);
