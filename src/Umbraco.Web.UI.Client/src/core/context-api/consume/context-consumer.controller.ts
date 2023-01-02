import { UmbContextConsumer } from './context-consumer';
import { UmbContextCallback } from './context-request.event';
import type { UmbController } from 'src/core/controller/controller.interface';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';


export class UmbContextConsumerController extends UmbContextConsumer implements UmbController {
   
    constructor(host:UmbControllerHostInterface, contextAlias: string, callback: UmbContextCallback) {
        super(host, contextAlias, callback);
        host.addController(this);
    }

}