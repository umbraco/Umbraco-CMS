import type { UmbClassInterface } from './class.interface.js';
import type { UmbControllerHost, UmbController } from '@umbraco-cms/backoffice/controller-api';

export interface UmbClassMixinInterface extends UmbClassInterface, UmbController {
	_host: UmbControllerHost;
}
