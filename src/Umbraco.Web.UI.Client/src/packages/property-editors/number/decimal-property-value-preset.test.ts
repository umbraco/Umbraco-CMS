import { UmbDecimalPropertyValuePreset } from './decimal-property-value-preset.js';
import { expect } from '@open-wc/testing';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';

describe('UmbDecimalPropertyValuePreset', () => {
	let preset: UmbDecimalPropertyValuePreset;

	beforeEach(() => {
		preset = new UmbDecimalPropertyValuePreset();
	});

	describe('with a configured default value', () => {
		const config: UmbPropertyEditorConfig = [{ alias: 'defaultValue', value: '5.5' }];

		it('uses the default value for a new property', async () => {
			expect(await preset.processValue(undefined, config)).to.equal(5.5);
		});

		it('preserves an existing value over the default value', async () => {
			expect(await preset.processValue(2.5, config)).to.equal(2.5);
		});

		it('preserves an existing value of 0 over the default value', async () => {
			expect(await preset.processValue(0, config)).to.equal(0);
		});

		it('applies a configured default value of 0', async () => {
			const zeroDefaultConfig: UmbPropertyEditorConfig = [{ alias: 'defaultValue', value: '0' }];
			const minConfig: UmbPropertyEditorConfig = [
				{ alias: 'defaultValue', value: '0' },
				{ alias: 'min', value: '10' },
			];

			expect(await preset.processValue(undefined, zeroDefaultConfig)).to.equal(0);

			// A configured default of 0 must win over min, not be treated as "no default".
			expect(await preset.processValue(undefined, minConfig)).to.equal(0);
		});
	});

	describe('falling back to the minimum', () => {
		it('uses min when no default value is configured', async () => {
			const config: UmbPropertyEditorConfig = [{ alias: 'min', value: '3' }];
			expect(await preset.processValue(undefined, config)).to.equal(3);
		});

		it('falls back to min when the default value is an empty string', async () => {
			const config: UmbPropertyEditorConfig = [
				{ alias: 'defaultValue', value: '' },
				{ alias: 'min', value: '3' },
			];
			expect(await preset.processValue(undefined, config)).to.equal(3);
		});

		it('falls back to min when the default value is not a finite number', async () => {
			const config: UmbPropertyEditorConfig = [
				{ alias: 'defaultValue', value: 'not-a-number' },
				{ alias: 'min', value: '3' },
			];
			expect(await preset.processValue(undefined, config)).to.equal(3);
		});

		it('defaults to 0 when neither default value nor min is configured', async () => {
			expect(await preset.processValue(undefined, [])).to.equal(0);
		});
	});
});
