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
  // Configure the generated API client for authenticated calls to the Management API
  _host.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
    authContext?.configureClient(client);
  });
  //#endif
};

export const onUnload: UmbEntryPointOnUnload = (_host, _extensionRegistry) => {
  console.log("Goodbye from my extension 👋");
};
