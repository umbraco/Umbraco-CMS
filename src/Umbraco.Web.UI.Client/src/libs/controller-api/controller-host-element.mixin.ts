import { HTMLElementConstructor } from '../extension-api/types.js';
import { UmbControllerAlias } from './controller-alias.type.js';
import { UmbControllerHostBaseMixin } from './controller-host-base.mixin.js';
import { UmbControllerHost } from './controller-host.interface.js';
import type { UmbController } from './controller.interface.js';
import { UmbLocalizeController } from '@umbraco-cms/backoffice/localization-api';

export declare class UmbControllerHostElement extends HTMLElement implements UmbControllerHost {
	get localize(): UmbLocalizeController | undefined;
	hasController(controller: UmbController): boolean;
	getControllers(filterMethod: (ctrl: UmbController) => boolean): UmbController[];
	addController(controller: UmbController): void;
	removeControllerByAlias(alias: UmbControllerAlias): void;
	removeController(controller: UmbController): void;
	getHostElement(): EventTarget;
}

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UmbControllerHostElementMixin = <T extends HTMLElementConstructor>(superClass: T) => {
	class UmbControllerHostElementClass extends UmbControllerHostBaseMixin(superClass) implements UmbControllerHost {
		getHostElement(): EventTarget {
			return this;
		}

		connectedCallback() {
			super.connectedCallback?.();
			this.hostConnected();
		}

		disconnectedCallback() {
			super.disconnectedCallback?.();
			this.hostDisconnected();
		}
	}

	return UmbControllerHostElementClass as unknown as HTMLElementConstructor<UmbControllerHostElement> & T;
};

declare global {
	interface HTMLElement {
		connectedCallback(): void;
		disconnectedCallback(): void;
	}
}
