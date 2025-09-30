import type { ManifestElementAndApi } from '../types/index.js';
import type { UmbApi } from '../index.js';
import { createExtensionElementWithApi } from './create-extension-element-with-api.function.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

interface UmbExtensionApiBooleanTestElement extends UmbLitElement {
	isValidElementClassInstance(): boolean;
}

@customElement('umb-extension-api-true-test-element')
class UmbExtensionApiTrueTestElement extends UmbLitElement implements UmbExtensionApiBooleanTestElement {
	isValidElementClassInstance() {
		return true;
	}
}

@customElement('umb-extension-api-false-test-element')
class UmbExtensionApiFalseTestElement extends UmbLitElement implements UmbExtensionApiBooleanTestElement {
	isValidElementClassInstance() {
		return false;
	}
}

const elementJsModuleWithDefaultExport = {
	default: UmbExtensionApiTrueTestElement,
};

const elementJsModuleWithElementExport = {
	element: UmbExtensionApiTrueTestElement,
};

const elementJsModuleWithDefaultAndElementExport = {
	default: UmbExtensionApiFalseTestElement,
	element: UmbExtensionApiTrueTestElement,
};

interface UmbTestApi extends UmbApi {
	isValidApiClassInstance(): boolean;
}

class UmbTestApiTrue implements UmbTestApi {
	isValidApiClassInstance() {
		return true;
	}
	destroy() {}
}
class UmbTestApiFalse implements UmbTestApi {
	isValidApiClassInstance() {
		return true;
	}
	destroy() {}
}

const apiJsModuleWithDefaultExport = {
	default: UmbTestApiTrue,
};

const apiJsModuleWithApiExport = {
	api: UmbTestApiTrue,
};

const apiJsModuleWithDefaultAndApiExport = {
	default: UmbTestApiFalse,
	api: UmbTestApiTrue,
};

describe('Create Extension Element and Api Method', () => {
	it('Returns `undefined` when manifest does not have any correct properties', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiBooleanTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
		};

		const { element, api } = await createExtensionElementWithApi(manifest);
		expect(element).to.be.undefined;
		expect(api).to.be.undefined;
	});

	it('Returns fallback element instance when manifest does not provide element', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(apiJsModuleWithApiExport),
		};

		const { element, api } = await createExtensionElementWithApi(manifest, [], 'umb-extension-api-true-test-element');
		expect(element).to.not.be.undefined;
		expect(api).to.not.be.undefined;
		if (element) {
			expect(element.isValidElementClassInstance()).to.be.true;
		}
		if (api) {
			expect(api.isValidApiClassInstance()).to.be.true;
		}
	});

	it('Still returns fallback element instance when manifest does not provide element and manifest has a js with an api', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(apiJsModuleWithApiExport),
		};

		const { element, api } = await createExtensionElementWithApi(manifest, [], 'umb-extension-api-true-test-element');
		expect(element).to.not.be.undefined;
		expect(api).to.not.be.undefined;
		if (element) {
			expect(element.isValidElementClassInstance()).to.be.true;
		}
		if (api) {
			expect(api.isValidApiClassInstance()).to.be.true;
		}
	});

	it('Handles when `api` property contains a class constructor', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			elementName: 'umb-extension-api-true-test-element',
			api: () => Promise.resolve(apiJsModuleWithDefaultExport),
		};

		const { element, api } = await createExtensionElementWithApi(manifest);
		expect(element).to.not.be.undefined;
		expect(api).to.not.be.undefined;
		if (element) {
			expect(element.isValidElementClassInstance()).to.be.true;
		}
		if (api) {
			expect(api.isValidApiClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a default export', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(elementJsModuleWithDefaultExport),
			api: () => Promise.resolve(apiJsModuleWithDefaultExport),
		};

		const { element, api } = await createExtensionElementWithApi(manifest);
		expect(element).to.not.be.undefined;
		expect(api).to.not.be.undefined;
		if (element) {
			expect(element.isValidElementClassInstance()).to.be.true;
		}
		if (api) {
			expect(api.isValidApiClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a element export', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(elementJsModuleWithElementExport),
			api: () => Promise.resolve(apiJsModuleWithApiExport),
		};

		const { element, api } = await createExtensionElementWithApi(manifest);
		expect(element).to.not.be.undefined;
		expect(api).to.not.be.undefined;
		if (element) {
			expect(element.isValidElementClassInstance()).to.be.true;
		}
		if (api) {
			expect(api.isValidApiClassInstance()).to.be.true;
		}
	});

	it('Prioritizes api export from loader property', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement, UmbTestApi> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(elementJsModuleWithDefaultAndElementExport),
			api: () => Promise.resolve(apiJsModuleWithDefaultAndApiExport),
		};

		const { element, api } = await createExtensionElementWithApi(manifest);
		expect(element).to.not.be.undefined;
		expect(api).to.not.be.undefined;
		if (element) {
			expect(element.isValidElementClassInstance()).to.be.true;
		}
		if (api) {
			expect(api.isValidApiClassInstance()).to.be.true;
		}
	});
});
