import { loadManifestElement } from './load-manifest-element.function.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-test-element-true')
class UmbTestElementTrue extends UmbLitElement {
	isValidClassInstance() {
		return true;
	}
}

@customElement('umb-test-element-false')
class UmbTestElementFalse extends UmbLitElement {
	isValidClassInstance() {
		return false;
	}
}

const jsModuleWithDefaultExport = {
	default: UmbTestElementTrue,
};

const jsModuleWithElementExport = {
	element: UmbTestElementTrue,
};

const jsModuleWithDefaultAndElementExport = {
	default: UmbTestElementFalse,
	element: UmbTestElementTrue,
};

describe('loadManifestElement', () => {
	describe('class constructor', () => {
		it('returns the class constructor when passed directly', async () => {
			const result = await loadManifestElement(UmbTestElementTrue);
			expect(result).to.equal(UmbTestElementTrue);
		});
	});

	describe('dynamic import (function)', () => {
		it('returns the element class from default export', async () => {
			const result = await loadManifestElement(() => Promise.resolve(jsModuleWithDefaultExport));
			expect(result).to.equal(UmbTestElementTrue);
		});

		it('returns the element class from element export', async () => {
			const result = await loadManifestElement(() => Promise.resolve(jsModuleWithElementExport));
			expect(result).to.equal(UmbTestElementTrue);
		});

		it('prioritizes element export over default export', async () => {
			const result = await loadManifestElement(() => Promise.resolve(jsModuleWithDefaultAndElementExport));
			expect(result).to.equal(UmbTestElementTrue);
		});

		it('returns undefined when loader returns null', async () => {
			const result = await loadManifestElement(() => Promise.resolve(null as any));
			expect(result).to.be.undefined;
		});

		it('returns undefined when loader returns object without valid exports', async () => {
			const result = await loadManifestElement(() => Promise.resolve({ other: 'value' } as any));
			expect(result).to.be.undefined;
		});

		it('returns undefined when export is not a function', async () => {
			const result = await loadManifestElement(() => Promise.resolve({ default: 'not-a-function' } as any));
			expect(result).to.be.undefined;
		});
	});

	describe('static import (object)', () => {
		it('returns the element class from default export', async () => {
			const result = await loadManifestElement(jsModuleWithDefaultExport);
			expect(result).to.equal(UmbTestElementTrue);
		});

		it('returns the element class from element export', async () => {
			const result = await loadManifestElement(jsModuleWithElementExport);
			expect(result).to.equal(UmbTestElementTrue);
		});

		it('prioritizes element export over default export', async () => {
			const result = await loadManifestElement(jsModuleWithDefaultAndElementExport);
			expect(result).to.equal(UmbTestElementTrue);
		});

		it('returns undefined when object has no valid exports', async () => {
			const result = await loadManifestElement({ other: 'value' } as any);
			expect(result).to.be.undefined;
		});

		it('returns undefined when export is not a function', async () => {
			const result = await loadManifestElement({ default: 'not-a-function' } as any);
			expect(result).to.be.undefined;
		});
	});

	describe('edge cases', () => {
		it('returns undefined when passed null', async () => {
			const result = await loadManifestElement(null as any);
			expect(result).to.be.undefined;
		});

		it('returns undefined when passed undefined', async () => {
			const result = await loadManifestElement(undefined as any);
			expect(result).to.be.undefined;
		});
	});
});
