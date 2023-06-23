import { UmbControllerHostBaseMixin } from './controller-host-base.mixin.js';
import { UmbControllerHost } from './controller-host.interface.js';
import type { UmbControllerInterface } from './controller.interface.js';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export abstract class UmbController extends UmbControllerHostBaseMixin(class {}) implements UmbControllerInterface {
	protected host?: UmbControllerHost;

	private _alias?: string;
	public get unique() {
		return this._alias;
	}

	constructor(host: UmbControllerHost, alias?: string) {
		super();
		this.host = host;
		this._alias = alias;
		this.host.addController(this);
	}

	public destroy() {
		if (this.host) {
			this.host.removeController(this);
		}
		delete this.host;
	}
}
