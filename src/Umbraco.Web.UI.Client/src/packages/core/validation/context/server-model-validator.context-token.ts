import type { UmbServerModelValidatorContext } from './server-model-validator.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SERVER_MODEL_VALIDATOR_CONTEXT = new UmbContextToken<UmbServerModelValidatorContext>(
	'UmbServerModelValidationContext',
);
