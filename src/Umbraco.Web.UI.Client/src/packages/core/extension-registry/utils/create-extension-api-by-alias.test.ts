import { createExtensionApiByAlias } from './create-extension-api-by-alias.function.js';
import type { ManifestApi, UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { expect, fixture } from '@open-wc/testing';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHostElementElement } from '@umbraco-cms/backoffice/controller-api';

interface UmbExtensionApiBoolTestClass extends UmbApi {
	isValidClassInstance(): boolean;
}
class UmbExtensionApiTrueTestClass implements UmbExtensionApiBoolTestClass {
	isValidClassInstance() {
		return true;
	}
	destroy() {}
}

class UmbExtensionApiFalseTestClass implements UmbExtensionApiBoolTestClass {
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

describe('Create Extension Api By Alias Method', () => {
	let hostElement: UmbControllerHostElementElement;

	beforeEach(async () => {
		hostElement = await fixture(html`<umb-test-controller-host></umb-test-controller-host>`);
	});

	it('Returns `undefined` when manifest does not have any correct properties', (done) => {
		const manifest: ManifestApi = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
		};
		umbExtensionsRegistry.register(manifest);

		createExtensionApiByAlias<UmbExtensionApiBoolTestClass>(hostElement, manifest.alias, []).then(
			() => {
				umbExtensionsRegistry.unregister(manifest.alias);
				done(new Error('Should not resolve'));
			},
			() => {
				umbExtensionsRegistry.unregister(manifest.alias);
				done();
			},
		);
	});

	it('Handles when `api` property contains a class constructor', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			api: UmbExtensionApiTrueTestClass,
		};
		umbExtensionsRegistry.register(manifest);

		const api = await createExtensionApiByAlias<UmbExtensionApiBoolTestClass>(hostElement, manifest.alias, []);
		expect(api.isValidClassInstance()).to.be.true;

		umbExtensionsRegistry.unregister(manifest.alias);
	});

	it('Handles when `loader` has a default export', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithDefaultExport),
		};
		umbExtensionsRegistry.register(manifest);

		const api = await createExtensionApiByAlias<UmbExtensionApiBoolTestClass>(hostElement, manifest.alias, []);
		expect(api.isValidClassInstance()).to.be.true;

		umbExtensionsRegistry.unregister(manifest.alias);
	});

	it('Handles when `loader` has a api export', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithApiExport),
		};
		umbExtensionsRegistry.register(manifest);

		const api = await createExtensionApiByAlias<UmbExtensionApiBoolTestClass>(hostElement, manifest.alias, []);
		expect(api.isValidClassInstance()).to.be.true;

		umbExtensionsRegistry.unregister(manifest.alias);
	});

	it('Prioritizes api export from loader property', async () => {
		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.createManifestApi',
			name: 'pretty name',
			js: () => Promise.resolve(jsModuleWithDefaultAndApiExport),
		};
		umbExtensionsRegistry.register(manifest);

		const api = await createExtensionApiByAlias<UmbExtensionApiBoolTestClass>(hostElement, manifest.alias, []);
		expect(api.isValidClassInstance()).to.be.true;

		umbExtensionsRegistry.unregister(manifest.alias);
	});
});
