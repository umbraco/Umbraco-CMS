import { UmbContextProvider } from './context-provider';
import type { UmbController } from 'src/core/controller/controller.interface';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';


export class UmbContextProviderController extends UmbContextProvider implements UmbController {
   
    constructor(host:UmbControllerHostInterface, contextAlias: string, instance: unknown) {
        super(host, contextAlias, instance);
        host.addController(this);
    }

}