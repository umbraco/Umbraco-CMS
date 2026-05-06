import type {
  UmbEntryPointOnInit,
  UmbEntryPointOnUnload,
} from "@umbraco-cms/backoffice/extension-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { client } from "../api/client.gen.js";

// load up the manifests here
export const onInit: UmbEntryPointOnInit = (host, _extensionRegistry) => {
  // Wire the generated API client into the backoffice auth context.
  // configureClient() sets baseUrl + credentials, attaches the auth callback
  // (cookie-based, with automatic token refresh) and binds the default
  // response interceptors (401 retry, error notifications, etc.).
  // The entry point signature is synchronous, so we kick off the context
  // resolution and configure the client when it resolves — calls made before
  // that point will fall back to the baseUrl baked into client.gen.ts.
  host.getContext(UMB_AUTH_CONTEXT).then((authContext) => {
    if (!authContext) {
      console.warn("UMB_AUTH_CONTEXT not available — extension API client will not be authenticated");
      return;
    }
    authContext.configureClient(client);
  });

  console.log("Hello from my extension 🎉");
};

export const onUnload: UmbEntryPointOnUnload = (_host, _extensionRegistry) => {
  console.log("Goodbye from my extension 👋");
};
