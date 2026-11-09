import type { JsLoaderProperty } from '../types/index.js';
import type { UmbEntryPointModule } from './entry-point.interface.js';
import { hasInitExport, hasOnUnloadExport, loadManifestPlainJs } from '../functions/index.js';
import { expect } from '@open-wc/testing';

describe('UmbEntryPointModule', () => {
	it('accepts a module that only exports onInit', () => {
		const module: UmbEntryPointModule = { onInit: () => {} };
		expect(hasInitExport(module)).to.be.true;
		expect(hasOnUnloadExport(module)).to.be.false;
	});

	it('requires onInit, while the guards still tolerate a module without it', () => {
		// @ts-expect-error onInit is the hook every entry point is expected to implement.
		const module: UmbEntryPointModule = { onUnload: () => {} };
		expect(hasInitExport(module)).to.be.false;
		expect(hasOnUnloadExport(module)).to.be.true;
	});

	it('does not treat a hook that is not a function as an export', () => {
		expect(hasInitExport({ onInit: undefined })).to.be.false;
		expect(hasOnUnloadExport({ onUnload: undefined })).to.be.false;
		expect(hasInitExport({ onInit: 'onInit' })).to.be.false;
		expect(hasOnUnloadExport({ onUnload: 'onUnload' })).to.be.false;
	});

	it('loads a manifest loader for a module that only exports onInit', async () => {
		const js: JsLoaderProperty<UmbEntryPointModule> = () => Promise.resolve({ onInit: () => {} });
		const module = await loadManifestPlainJs(js);
		expect(hasInitExport(module)).to.be.true;
		expect(hasOnUnloadExport(module)).to.be.false;
	});
});
