import type {
  UmbEntryPointOnInit,
  UmbEntryPointOnUnload,
} from "@umbraco-cms/backoffice/extension-api";
//#if IncludeExample
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { client } from "../api/client.gen.js";
//#endif

// load up the manifests here
export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {
  console.log("Hello from my extension 🎉");
  //#if IncludeExample
  // Will use only to add in Open API config with generated TS OpenAPI HTTPS Client
  // Do the OAuth token handshake stuff
  _host.consumeContext(UMB_AUTH_CONTEXT, async (authContext) => {
    // Get the token info from Umbraco
    const config = authContext?.getOpenApiConfiguration();

    client.setConfig({
      auth: config?.token ?? undefined,
      baseUrl: config?.base ?? "",
      credentials: config?.credentials ?? "same-origin",
    });
  });
  //#endif
};

export const onUnload: UmbEntryPointOnUnload = (_host, _extensionRegistry) => {
  console.log("Goodbye from my extension 👋");
};
