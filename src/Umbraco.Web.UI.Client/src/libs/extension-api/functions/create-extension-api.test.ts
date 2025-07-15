import type { ManifestApi } from '../types/index.js';
import type { UmbApi } from '../models/api.interface.js';
import { createExtensionApi } from './create-extension-api.function.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbExtensionApiTrueTestClass implements UmbApi {
	isValidClassInstance() {
		return true;
	}
	destroy() {}
}

class UmbExtensionApiFalseTestClass implements UmbApi {
	isValidClassInstance() {
		return false;
	}
	destroy() {}
}

const jsModuleWithDefaultExport = {
	default: UmbExtensionApiTrueTestClass,
};

const jsModuleWithApiExport = {
	api: UmbExtensionApiTrueTestClass,
};

const jsModuleWithDefaultAndApiExport = {
	default: UmbExtensionApiFalseTestClass,
	api: UmbExtensionApiTrueTestClass,
};

describe('Create Extension Api Method', () => {
	let hostElement: UmbTestControllerHostElement;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
	});

	it('Returns `undefined` when manifest does not have any correct properties', async () => {
		const manifest: ManifestApi = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
		};

		const api = await createExtensionApi(hostElement, manifest, []);
		expect(api).to.be.undefined;
	});

	it('Handles when `api` property contains a class constructor', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			api: UmbExtensionApiTrueTestClass,
		};

		const api = await createExtensionApi(hostElement, manifest, []);
		expect(api).to.not.be.undefined;
		if (api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a default export', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithDefaultExport),
		};

		const api = await createExtensionApi(hostElement, manifest, []);
		expect(api).to.not.be.undefined;
		if (api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a api export', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithApiExport),
		};

		const api = await createExtensionApi(hostElement, manifest, []);
		expect(api).to.not.be.undefined;
		if (api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	it('Prioritizes api export from loader property', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithDefaultAndApiExport),
		};

		const api = await createExtensionApi(hostElement, manifest, []);
		expect(api).to.not.be.undefined;
		if (api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});
});
