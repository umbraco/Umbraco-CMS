import { UmbClassMixin } from './class.mixin.js';
import type { UmbController } from '@umbraco-cms/backoffice/controller-api';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export abstract class UmbControllerBase
	extends UmbClassMixin<ClassConstructor<EventTarget>>(EventTarget)
	implements UmbController {}
