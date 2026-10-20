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

	it('accepts a module that only exports onUnload', () => {
		const module: UmbEntryPointModule = { onUnload: () => {} };
		expect(hasInitExport(module)).to.be.false;
		expect(hasOnUnloadExport(module)).to.be.true;
	});

	it('does not treat a hook that is not a function as an export', () => {
		const module: UmbEntryPointModule = { onInit: undefined, onUnload: undefined };
		expect(hasInitExport(module)).to.be.false;
		expect(hasOnUnloadExport(module)).to.be.false;
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
