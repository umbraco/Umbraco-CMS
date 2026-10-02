import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * Switches the mock server to the "Multi Brand Clothing Shop" data set and hides the mock set switcher.
 */
export async function onInit() {
	if (import.meta.env.VITE_UMBRACO_USE_MSW !== 'on') return;

	const { useMockSet } = await import('@umbraco-cms/internal/mock-manager');
	await useMockSet('multiBrandClothingShop');
	umbExtensionsRegistry.exclude('Mock.HeaderApp.MockSetSwitcher');
}

/**
 *
 */
export function onUnload() {}
