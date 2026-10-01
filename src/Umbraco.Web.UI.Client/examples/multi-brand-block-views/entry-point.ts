import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export function onInit() {
	umbExtensionsRegistry.exclude('Mock.HeaderApp.MockSetSwitcher');
}
