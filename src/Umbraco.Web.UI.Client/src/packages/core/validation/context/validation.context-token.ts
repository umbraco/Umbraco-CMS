import type { UmbValidationController } from '../controllers/validation.controller.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VALIDATION_CONTEXT = new UmbContextToken<UmbValidationController>('UmbValidationContext');
