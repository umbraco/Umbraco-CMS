import { expect } from '@open-wc/testing';
import { ManifestApi, UmbApi } from '../types.js';
import { createExtensionApi } from './create-extension-api.function.js';




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
	default: UmbExtensionApiTrueTestClass
};

const jsModuleWithApiExport = {
	api: UmbExtensionApiTrueTestClass
};

const jsModuleWithDefaultAndApiExport = {
	default: UmbExtensionApiFalseTestClass,
	api: UmbExtensionApiTrueTestClass
};



describe('Extension-Api: Create Extension Api', () => {



	it('Returns `undefined` when manifest does not have any correct properties', async () => {

		const manifest: ManifestApi = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateExtensionApi',
			name: 'pretty name'
		};

		const api = await createExtensionApi(manifest, []);
		expect(api).to.be.undefined;
	});

	it('Handles when `api` property contains a class constructor', async () => {

		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateExtensionApi',
			name: 'pretty name',
			api: UmbExtensionApiTrueTestClass
		};

		const api = await createExtensionApi(manifest, []);
		expect(api).to.not.be.undefined;
		if(api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a default export', async () => {

		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateExtensionApi',
			name: 'pretty name',
			loader: () => Promise.resolve(jsModuleWithDefaultExport)
		};

		const api = await createExtensionApi(manifest, []);
		expect(api).to.not.be.undefined;
		if(api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	it('Handles when `loader` has a api export', async () => {

		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateExtensionApi',
			name: 'pretty name',
			loader: () => Promise.resolve(jsModuleWithApiExport)
		};

		const api = await createExtensionApi(manifest, []);
		expect(api).to.not.be.undefined;
		if(api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	it('Prioritizes api export from loader property', async () => {

		const manifest: ManifestApi<UmbExtensionApiTrueTestClass> = {
			type: 'my-test-type',
			alias: 'Umb.Test.CreateExtensionApi',
			name: 'pretty name',
			loader: () => Promise.resolve(jsModuleWithDefaultAndApiExport)
		};

		const api = await createExtensionApi(manifest, []);
		expect(api).to.not.be.undefined;
		if(api) {
			expect(api.isValidClassInstance()).to.be.true;
		}
	});

	//TODO: Test apiJs
	//TODO: Test js

});
