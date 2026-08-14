import type { UmbValueSummaryResolver } from './value-summary-resolver.interface.js';
import { loadValueSummaryResolver } from './load-value-summary-resolver.function.js';
import { expect } from '@open-wc/testing';

class TestResolverA implements UmbValueSummaryResolver {
	resolveValues = async (values: ReadonlyArray<unknown>) => ({ data: [...values] });
	destroy() {}
}

class TestResolverB implements UmbValueSummaryResolver {
	resolveValues = async (values: ReadonlyArray<unknown>) => ({ data: [...values] });
	destroy() {}
}

const moduleWithValueResolverExport = { valueResolver: TestResolverA };
const moduleWithDefaultExport = { default: TestResolverA };
const moduleWithBothExports = { valueResolver: TestResolverA, default: TestResolverB };

describe('loadValueSummaryResolver', () => {
	describe('class constructor', () => {
		it('returns the class when a constructor is passed directly', async () => {
			const result = await loadValueSummaryResolver(TestResolverA);
			expect(result).to.equal(TestResolverA);
		});
	});

	describe('dynamic import (function)', () => {
		it('returns the resolver class from a valueResolver export', async () => {
			const result = await loadValueSummaryResolver(() => Promise.resolve(moduleWithValueResolverExport));
			expect(result).to.equal(TestResolverA);
		});

		it('returns the resolver class from a default export', async () => {
			const result = await loadValueSummaryResolver(() => Promise.resolve(moduleWithDefaultExport));
			expect(result).to.equal(TestResolverA);
		});

		it('prioritizes valueResolver export over default export', async () => {
			const result = await loadValueSummaryResolver(() => Promise.resolve(moduleWithBothExports));
			expect(result).to.equal(TestResolverA);
		});

		it('returns undefined when the module has no recognised export', async () => {
			const result = await loadValueSummaryResolver(() => Promise.resolve({ other: 'value' } as any));
			expect(result).to.be.undefined;
		});

		it('returns undefined when the export is not a function', async () => {
			const result = await loadValueSummaryResolver(() => Promise.resolve({ valueResolver: 'not-a-class' } as any));
			expect(result).to.be.undefined;
		});

		it('returns undefined when the module resolves to null', async () => {
			const result = await loadValueSummaryResolver(() => Promise.resolve(null as any));
			expect(result).to.be.undefined;
		});
	});

	describe('edge cases', () => {
		it('returns undefined when passed null', async () => {
			const result = await loadValueSummaryResolver(null as any);
			expect(result).to.be.undefined;
		});

		it('returns undefined when passed undefined', async () => {
			const result = await loadValueSummaryResolver(undefined as any);
			expect(result).to.be.undefined;
		});
	});
});
