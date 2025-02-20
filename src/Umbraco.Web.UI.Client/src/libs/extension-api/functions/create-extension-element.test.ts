import type { ManifestElement, ManifestElementAndApi } from '../types/index.js';
import { createExtensionElement } from './create-extension-element.function.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-extension-api-true-test-element')
class UmbExtensionApiTrueTestElement extends UmbLitElement {
	isValidClassInstance() {
		return true;
	}
}

@customElement('umb-extension-api-false-test-element')
class UmbExtensionApiFalseTestElement extends UmbLitElement {
	isValidClassInstance() {
		return false;
	}
}

const jsModuleWithDefaultExport = {
	default: UmbExtensionApiTrueTestElement,
};

const jsModuleWithElementExport = {
	element: UmbExtensionApiTrueTestElement,
};

const jsModuleWithDefaultAndElementExport = {
	default: UmbExtensionApiFalseTestElement,
	element: UmbExtensionApiTrueTestElement,
};

describe('Create Extension Element Method', () => {
	it('Returns `undefined` when manifest does not have any correct properties', async () => {
		const manifest: ManifestElement = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
		};

		const api = await createExtensionElement(manifest);
		expect(api).to.be.undefined;
	});

	it('Returns fallback element instance when manifest does not provide element', async () => {
		const manifest: ManifestElement<UmbExtensionApiTrueTestElement> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
		};

		const element = await createExtensionElement(manifest, 'umb-extension-api-true-test-element');
		expect(element).to.not.be.undefined;
		if (element) {
			expect(element.isValidClassInstance()).to.be.true;
		}
	});

	it('Still returns fallback element instance when manifest does not provide element and manifest has api', async () => {
		const manifest: ManifestElementAndApi<UmbExtensionApiTrueTestElement> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			api: class TestApi {
				destroy() {}
			},
		};

		const element = await createExtensionElement(manifest, 'umb-extension-api-true-test-element');
		expect(element).to.not.be.undefined;
		if (element) {
			expect(element.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `api` property contains a class constructor', async () => {
		const manifest: ManifestElement<UmbExtensionApiTrueTestElement> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			elementName: 'umb-extension-api-true-test-element',
		};

		const element = await createExtensionElement(manifest);
		expect(element).to.not.be.undefined;
		if (element) {
			expect(element.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a default export', async () => {
		const manifest: ManifestElement<UmbExtensionApiTrueTestElement> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithDefaultExport),
		};

		const element = await createExtensionElement(manifest);
		expect(element).to.not.be.undefined;
		if (element) {
			expect(element.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a element export', async () => {
		const manifest: ManifestElement<UmbExtensionApiTrueTestElement> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithElementExport),
		};

		const element = await createExtensionElement(manifest);
		expect(element).to.not.be.undefined;
		if (element) {
			expect(element.isValidClassInstance()).to.be.true;
		}
	});

	it('Prioritizes api export from loader property', async () => {
		const manifest: ManifestElement<UmbExtensionApiTrueTestElement> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateManifestElement',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithDefaultAndElementExport),
		};

		const element = await createExtensionElement(manifest);
		expect(element).to.not.be.undefined;
		if (element) {
			expect(element.isValidClassInstance()).to.be.true;
		}
	});

	//TODO: Test elementJs
	//TODO: Test js
});
