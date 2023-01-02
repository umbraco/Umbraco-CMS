import { UmbContextProvider } from './context-provider';
import type { UmbControllerInterface } from 'src/core/controller/controller.interface';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';


export class UmbContextProviderController extends UmbContextProvider implements UmbControllerInterface {
   
    constructor(host:UmbControllerHostInterface, contextAlias: string, instance: unknown) {
        super(host, contextAlias, instance);

        // TODO: What if this API is already provided with this alias? maybe handle this in the controller.
        
        host.addController(this);
    }

}