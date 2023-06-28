import { UmbClassMixin } from '../class-api/index.js';
import { UmbControllerHost } from './controller-host.interface.js';
import { UmbController } from './controller.interface.js';

/**
 * This mixin enables a web-component to host controllers.
 * This enables controllers to be added to the life cycle of this element.
 *
 */
export class UmbBaseController extends UmbClassMixin(class {}) implements UmbController {
	private _controllerAlias?: string;
	public get unique() {
		return this._controllerAlias;
	}

	constructor(host: UmbControllerHost, alias?: string) {
		super(host);
		this._controllerAlias = alias;
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
