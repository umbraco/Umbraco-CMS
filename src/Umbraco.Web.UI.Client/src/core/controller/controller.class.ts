import { UmbControllerHostInterface } from './controller-host.mixin';
import { UmbControllerInterface } from './controller.interface';

export abstract class UmbController implements UmbControllerInterface {
	protected host?: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.host = host;
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
