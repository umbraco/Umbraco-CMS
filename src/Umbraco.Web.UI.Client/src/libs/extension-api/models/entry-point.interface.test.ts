import type { ManifestPlainJs } from '../types/index.js';
import type { UmbEntryPointModule } from './entry-point.interface.js';
import { hasInitExport, hasOnUnloadExport } from '../functions/index.js';
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

	it('accepts a manifest loader for a module that only exports onInit', async () => {
		const manifest: ManifestPlainJs<UmbEntryPointModule> = {
			type: 'backofficeEntryPoint',
			alias: 'Umb.Test.EntryPoint.OnInitOnly',
			name: 'Test entry point exporting only onInit',
			js: () => Promise.resolve({ onInit: () => {} }),
		};
		expect(manifest.js).to.be.a('function');
	});
});
