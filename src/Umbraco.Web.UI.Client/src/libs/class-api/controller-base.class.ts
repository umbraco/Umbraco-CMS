import type { UmbController } from '../controller-api/controller.interface.js';
import { UmbClassMixin } from './class.mixin.js';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export abstract class UmbBaseController extends UmbClassMixin(EventTarget) implements UmbController {}
