import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

// load up the manifests here
export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {

  console.log('Hello from my extension 🎉');

  // We can use extensionRegistry to register one or manifests/extensions via code
  // as opposed to a very long umbraco-package.json file
  // https://docs.umbraco.com/umbraco-cms/customizing/extending-overview/extension-types/backoffice-entry-point
  //_extensionRegistry.registerMany([
  //]);

  
};
