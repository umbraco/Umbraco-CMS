import { UmbClassMixin } from '../class-api/index.js';
import { UmbControllerHost } from './controller-host.interface.js';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export class UmbController extends UmbClassMixin(class {}) {
	private _alias?: string;
	public get unique() {
		return this._alias;
	}

	constructor(host: UmbControllerHost, alias?: string) {
		super(host);
		this._alias = alias;
		this._host.addController(this);
	}

	public destroy() {
		if (this._host) {
			this._host.removeController(this);
		}
		//delete this.host;
		super.destroy();
	}
}
