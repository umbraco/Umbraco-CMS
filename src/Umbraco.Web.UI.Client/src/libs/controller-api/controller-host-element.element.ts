import { UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * A web-component to host umb controllers.
 * This enables controllers to be added to the life cycle of this element.
 */
@customElement('umb-controller-host')
export class UmbControllerHostElementElement extends UmbControllerHostElementMixin(HTMLElement) {}
