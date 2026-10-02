import { umbMockManager } from '../../mocks/mock-manager.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const MOCK_SET = 'multiBrandClothingShop';
const MOCK_SET_STORAGE_KEY = 'umb:mockSet';

export function onInit() {
	umbExtensionsRegistry.exclude('Mock.HeaderApp.MockSetSwitcher');

	if (umbMockManager.currentSetName !== MOCK_SET) {
		localStorage.setItem(MOCK_SET_STORAGE_KEY, MOCK_SET);
		window.location.reload();
	}
}

export function onUnload() {}
