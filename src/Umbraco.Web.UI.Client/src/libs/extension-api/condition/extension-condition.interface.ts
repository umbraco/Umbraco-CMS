import type { UmbConditionConfigBase } from '../types/index.js';
import type { UmbController } from '@umbraco-cms/backoffice/controller-api';

export interface UmbExtensionCondition extends UmbController {
	/**
	 * Whether the condition permits the extension. Undefined until the condition has given its first answer.
	 */
	readonly permitted?: boolean;
	readonly config: UmbConditionConfigBase;
}
