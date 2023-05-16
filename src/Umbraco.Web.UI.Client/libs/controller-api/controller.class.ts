import { UmbControllerHostElement } from './controller-host.mixin';
import { UmbControllerInterface } from './controller.interface';

export abstract class UmbController implements UmbControllerInterface {
	protected host?: UmbControllerHostElement;

	private _alias?: string;
	public get unique() {
		return this._alias;
	}

	constructor(host: UmbControllerHostElement, alias?: string) {
		this.host = host;
		this._alias = alias;
		this.host.addController(this);
	}

	abstract hostConnected(): void;
	abstract hostDisconnected(): void;

	public destroy() {
		if (this.host) {
			this.host.removeController(this);
		}
		delete this.host;
	}
}
