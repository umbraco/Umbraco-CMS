import type { UmbConditionConfigBase } from '../types/index.js';
import type { UmbController } from '@umbraco-cms/backoffice/controller-api';

export interface UmbExtensionCondition extends UmbController {
	readonly permitted: boolean;
	readonly config: UmbConditionConfigBase;
}
