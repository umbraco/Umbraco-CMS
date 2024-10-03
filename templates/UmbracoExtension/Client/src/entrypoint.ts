import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

// load up the manifests here
export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {

  console.log('Hello from my extension');

  //// We can register many manifests at once via code
  //// as opposed to a long umbraco-package.json file
  //extensionRegistry.registerMany([
  //]);
};
