import type { UmbConditionConfig } from '../types.js';
import { UmbController } from '@umbraco-cms/backoffice/controller-api';

export interface UmbExtensionCondition extends UmbController {
	readonly permitted: boolean;
	readonly config: UmbConditionConfig;
}
