import { UmbContextConsumer } from './context-consumer';
import { UmbContextCallback } from './context-request.event';
import type { UmbControllerInterface } from 'src/core/controller/controller.interface';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';


export class UmbContextConsumerController extends UmbContextConsumer<UmbControllerHostInterface> implements UmbControllerInterface {

	public get unique() {
		return this._contextAlias;
	}

	constructor(host:UmbControllerHostInterface, contextAlias: string, callback: UmbContextCallback) {
		super(host, contextAlias, callback);
		host.addController(this);
	}

	public destroy() {
		if (this.host) {
			this.host.removeController(this);
		}
	}

}
