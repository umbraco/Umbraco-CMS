import { UmbControllerHostInterface } from './controller-host.mixin';
import { UmbControllerInterface } from './controller.interface';

export abstract class UmbController implements UmbControllerInterface {
	protected _host?: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this._host = host;
		this._host.addController(this);
	}

	abstract hostConnected(): void;
	abstract hostDisconnected(): void;

	public destroy() {
		if (this._host) {
			this._host.removeController(this);
		}
		delete this._host;
	}
}
