import { loadManifestPlainJs } from './load-manifest-plain-js.function.js';
import { expect } from '@open-wc/testing';

interface TestModule {
	value: string;
	getValue(): string;
}

const testModuleObject: TestModule = {
	value: 'test-value',
	getValue() {
		return this.value;
	},
};

const jsModuleWithTestExport = {
	value: 'dynamic-import-value',
	getValue() {
		return this.value;
	},
};

describe('loadManifestPlainJs', () => {
	describe('dynamic import (function)', () => {
		it('returns the module when loader returns an object', async () => {
			const result = await loadManifestPlainJs<TestModule>(() => Promise.resolve(jsModuleWithTestExport));
			expect(result).to.not.be.undefined;
			expect(result?.value).to.equal('dynamic-import-value');
			expect(result?.getValue()).to.equal('dynamic-import-value');
		});

		it('returns undefined when loader returns null', async () => {
			const result = await loadManifestPlainJs<TestModule>(() => Promise.resolve(null as unknown as TestModule));
			expect(result).to.be.undefined;
		});

		it('returns undefined when loader returns a primitive', async () => {
			const result = await loadManifestPlainJs<TestModule>(() => Promise.resolve('string' as unknown as TestModule));
			expect(result).to.be.undefined;
		});
	});

	describe('static import (object)', () => {
		it('returns the module object directly when passed an object', async () => {
			const result = await loadManifestPlainJs<TestModule>(testModuleObject);
			expect(result).to.not.be.undefined;
			expect(result?.value).to.equal('test-value');
			expect(result?.getValue()).to.equal('test-value');
		});

		it('returns undefined when passed null', async () => {
			const result = await loadManifestPlainJs<TestModule>(null as unknown as TestModule);
			expect(result).to.be.undefined;
		});
	});

	describe('edge cases', () => {
		it('returns undefined when passed undefined', async () => {
			const result = await loadManifestPlainJs<TestModule>(undefined as unknown as TestModule);
			expect(result).to.be.undefined;
		});
	});
});
