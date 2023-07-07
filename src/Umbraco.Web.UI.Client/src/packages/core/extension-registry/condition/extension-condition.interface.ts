import { UmbController } from '@umbraco-cms/backoffice/controller-api';

export interface UmbExtensionCondition extends UmbController {
	readonly permitted: boolean;
}
