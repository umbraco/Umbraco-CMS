import {
  UmbBundleExtensionInitializer,
  UmbEntryPointExtensionInitializer
} from "@umbraco-cms/backoffice/extension-api";
import { umbExtensionsRegistry } from "@umbraco-cms/backoffice/extension-registry";
import type { UmbElement } from "@umbraco-cms/backoffice/element-api";
import { UmbControllerBase } from "@umbraco-cms/backoffice/class-api";
import { UmbIconRegistry } from "@umbraco-cms/backoffice/icon";
import { UUIIconRegistryEssential } from "@umbraco-cms/backoffice/external/uui";

/**
 * This is the initializer for the slim backoffice.
 * It is responsible for initializing the backoffice and only the extensions that is needed to run the login screen.
 */
export class UmbSlimBackofficeController extends UmbControllerBase {
  #umbIconRegistry = new UmbIconRegistry();
  #uuiIconRegistry = new UUIIconRegistryEssential();

  constructor(host: UmbElement) {
    super(host);
    new UmbBundleExtensionInitializer(host, umbExtensionsRegistry);
    new UmbEntryPointExtensionInitializer(host, umbExtensionsRegistry);
    //new UmbServerExtensionRegistrator(host, umbExtensionsRegistry); // TODO: We need an unauthorized endpoint to register public extensions.

    this.#umbIconRegistry.attach(host);
    this.#uuiIconRegistry.attach(host);

    host.classList.add('uui-text');
    host.classList.add('uui-font');
  }
}
