import type { UmbApi } from '../models/api.interface.js';
import { loadManifestApi } from './load-manifest-api.function.js';
import { expect } from '@open-wc/testing';

class UmbTestApiTrue implements UmbApi {
	isValidClassInstance() {
		return true;
	}
	destroy() {}
}

class UmbTestApiFalse implements UmbApi {
	isValidClassInstance() {
		return false;
	}
	destroy() {}
}

const jsModuleWithDefaultExport = {
	default: UmbTestApiTrue,
};

const jsModuleWithApiExport = {
	api: UmbTestApiTrue,
};

const jsModuleWithDefaultAndApiExport = {
	default: UmbTestApiFalse,
	api: UmbTestApiTrue,
};

describe('loadManifestApi', () => {
	describe('class constructor', () => {
		it('returns the class constructor when passed directly', async () => {
			const result = await loadManifestApi(UmbTestApiTrue);
			expect(result).to.equal(UmbTestApiTrue);
		});
	});

	describe('dynamic import (function)', () => {
		it('returns the api class from default export', async () => {
			const result = await loadManifestApi(() => Promise.resolve(jsModuleWithDefaultExport));
			expect(result).to.equal(UmbTestApiTrue);
		});

		it('returns the api class from api export', async () => {
			const result = await loadManifestApi(() => Promise.resolve(jsModuleWithApiExport));
			expect(result).to.equal(UmbTestApiTrue);
		});

		it('prioritizes api export over default export', async () => {
			const result = await loadManifestApi(() => Promise.resolve(jsModuleWithDefaultAndApiExport));
			expect(result).to.equal(UmbTestApiTrue);
		});

		it('returns undefined when loader returns null', async () => {
			const result = await loadManifestApi(() => Promise.resolve(null as any));
			expect(result).to.be.undefined;
		});

		it('returns undefined when loader returns object without valid exports', async () => {
			const result = await loadManifestApi(() => Promise.resolve({ other: 'value' } as any));
			expect(result).to.be.undefined;
		});

		it('returns undefined when export is not a function', async () => {
			const result = await loadManifestApi(() => Promise.resolve({ default: 'not-a-function' } as any));
			expect(result).to.be.undefined;
		});
	});

	describe('static import (object)', () => {
		it('returns the api class from default export', async () => {
			const result = await loadManifestApi(jsModuleWithDefaultExport);
			expect(result).to.equal(UmbTestApiTrue);
		});

		it('returns the api class from api export', async () => {
			const result = await loadManifestApi(jsModuleWithApiExport);
			expect(result).to.equal(UmbTestApiTrue);
		});

		it('prioritizes api export over default export', async () => {
			const result = await loadManifestApi(jsModuleWithDefaultAndApiExport);
			expect(result).to.equal(UmbTestApiTrue);
		});

		it('returns undefined when object has no valid exports', async () => {
			const result = await loadManifestApi({ other: 'value' } as any);
			expect(result).to.be.undefined;
		});

		it('returns undefined when export is not a function', async () => {
			const result = await loadManifestApi({ default: 'not-a-function' } as any);
			expect(result).to.be.undefined;
		});
	});

	describe('edge cases', () => {
		it('returns undefined when passed null', async () => {
			const result = await loadManifestApi(null as any);
			expect(result).to.be.undefined;
		});

		it('returns undefined when passed undefined', async () => {
			const result = await loadManifestApi(undefined as any);
			expect(result).to.be.undefined;
		});
	});
});
