import { expect } from '@open-wc/testing';
import { UmbIntegerPropertyValuePreset } from './integer-property-value-preset.js';
import { UmbDecimalPropertyValuePreset } from './decimal-property-value-preset.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

const config = (values: Record<string, unknown>): UmbPropertyEditorConfig =>
	Object.entries(values).map(([alias, value]) => ({ alias, value }));

describe('UmbIntegerPropertyValuePreset', () => {
	const preset = new UmbIntegerPropertyValuePreset();
	const process = (value: number | undefined, cfg: UmbPropertyEditorConfig = []) => preset.processValue(value, cfg);

	describe('min configuration', () => {
		it('keeps the value undefined when min is zero (#21787)', async () => {
			expect(await process(undefined, config({ min: 0 }))).to.equal(undefined);
		});

		it('keeps the value undefined when min is undefined', async () => {
			expect(await process(undefined, [])).to.equal(undefined);
		});

		it('uses min when min is higher than zero', async () => {
			expect(await process(undefined, config({ min: 5 }))).to.equal(5);
		});

		it('keeps the value undefined when min is negative (zero is within range)', async () => {
			expect(await process(undefined, config({ min: -5 }))).to.equal(undefined);
		});
	});

	describe('max configuration (min undefined)', () => {
		it('keeps the value undefined when max is zero', async () => {
			expect(await process(undefined, config({ max: 0 }))).to.equal(undefined);
		});

		it('keeps the value undefined when max is a positive number', async () => {
			expect(await process(undefined, config({ max: 10 }))).to.equal(undefined);
		});

		it('uses max when max is a negative number', async () => {
			expect(await process(undefined, config({ max: -5 }))).to.equal(-5);
		});
	});

	describe('min and max combined', () => {
		it('prefers min when min is above zero', async () => {
			expect(await process(undefined, config({ min: 5, max: 10 }))).to.equal(5);
		});

		it('uses max when the whole range is below zero', async () => {
			expect(await process(undefined, config({ min: -10, max: -5 }))).to.equal(-5);
		});

		it('keeps the value undefined when zero is within the range', async () => {
			expect(await process(undefined, config({ min: -10, max: 10 }))).to.equal(undefined);
		});
	});

	describe('rounding and parsing', () => {
		it('rounds a fractional min to a whole number', async () => {
			expect(await process(undefined, config({ min: 2.6 }))).to.equal(3);
		});

		it('rounds a fractional negative max to a whole number', async () => {
			expect(await process(undefined, config({ max: -2.4 }))).to.equal(-2);
		});

		it('parses string config values', async () => {
			expect(await process(undefined, config({ min: '5' }))).to.equal(5);
		});

		it('keeps the value undefined when min is an empty string', async () => {
			expect(await process(undefined, config({ min: '' }))).to.equal(undefined);
		});

		it('keeps the value undefined when min is null', async () => {
			expect(await process(undefined, config({ min: null }))).to.equal(undefined);
		});

		it('keeps the value undefined when min is not a number', async () => {
			expect(await process(undefined, config({ min: 'abc' }))).to.equal(undefined);
		});
	});

	describe('existing values', () => {
		it('does not override an existing value', async () => {
			expect(await process(42, config({ min: 5 }))).to.equal(42);
		});

		it('does not override an existing value of zero', async () => {
			expect(await process(0, config({ min: 5 }))).to.equal(0);
		});
	});
});

describe('UmbDecimalPropertyValuePreset', () => {
	const preset = new UmbDecimalPropertyValuePreset();
	const process = (value: number | undefined, cfg: UmbPropertyEditorConfig = []) => preset.processValue(value, cfg);

	describe('min configuration', () => {
		it('keeps the value undefined when min is zero (#21787)', async () => {
			expect(await process(undefined, config({ min: 0 }))).to.equal(undefined);
		});

		it('keeps the value undefined when min is undefined', async () => {
			expect(await process(undefined, [])).to.equal(undefined);
		});

		it('uses min when min is higher than zero, keeping the fractional part', async () => {
			expect(await process(undefined, config({ min: 2.5 }))).to.equal(2.5);
		});

		it('keeps the value undefined when min is negative (zero is within range)', async () => {
			expect(await process(undefined, config({ min: -2.5 }))).to.equal(undefined);
		});
	});

	describe('max configuration (min undefined)', () => {
		it('keeps the value undefined when max is zero', async () => {
			expect(await process(undefined, config({ max: 0 }))).to.equal(undefined);
		});

		it('keeps the value undefined when max is a positive number', async () => {
			expect(await process(undefined, config({ max: 10.5 }))).to.equal(undefined);
		});

		it('uses max when max is a negative number, keeping the fractional part', async () => {
			expect(await process(undefined, config({ max: -2.5 }))).to.equal(-2.5);
		});
	});

	describe('min and max combined', () => {
		it('prefers min when min is above zero', async () => {
			expect(await process(undefined, config({ min: 2.5, max: 10 }))).to.equal(2.5);
		});

		it('uses max when the whole range is below zero', async () => {
			expect(await process(undefined, config({ min: -10, max: -2.5 }))).to.equal(-2.5);
		});

		it('keeps the value undefined when zero is within the range', async () => {
			expect(await process(undefined, config({ min: -10, max: 10 }))).to.equal(undefined);
		});
	});

	describe('existing values', () => {
		it('does not override an existing value', async () => {
			expect(await process(9.9, config({ min: 2.5 }))).to.equal(9.9);
		});

		it('does not override an existing value of zero', async () => {
			expect(await process(0, config({ min: 2.5 }))).to.equal(0);
		});
	});
});
