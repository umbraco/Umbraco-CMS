import { UmbClassMixin } from '../class-api/index.js';
import { UmbController } from './controller.interface.js';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export abstract class UmbBaseController extends UmbClassMixin(class {}) implements UmbController {}
