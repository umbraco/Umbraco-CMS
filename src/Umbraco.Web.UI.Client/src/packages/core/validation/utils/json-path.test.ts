import { expect } from '@open-wc/testing';
import { GetValueByJsonPath } from './json-path.function.js';

describe('UmbJsonPathFunctions', () => {
	it('retrieves root when path is root', () => {
		const data = { value: 'test' };
		const result = GetValueByJsonPath(data, '$') as any;

		expect(result).to.eq(data);
		expect(result.value).to.eq('test');
	});

	it('retrieve property value', () => {
		const result = GetValueByJsonPath({ value: 'test' }, '$.value');

		expect(result).to.eq('test');
	});

	it('value of first entry in an array', () => {
		const result = GetValueByJsonPath({ values: ['test'] }, '$.values[0]');

		expect(result).to.eq('test');
	});

	it('value property of first entry in an array', () => {
		const result = GetValueByJsonPath({ values: [{ value: 'test' }] }, '$.values[0].value');

		expect(result).to.eq('test');
	});

	it('value property of first entry in an array', () => {
		const result = GetValueByJsonPath(
			{ values: [{ value: { deepData: [{ value: 'inner' }] } }] },
			'$.values[0].value.deepData[0].value',
		);

		expect(result).to.eq('inner');
	});

	it('query of first entry in an array', () => {
		const result = GetValueByJsonPath({ values: [{ id: '123', value: 'test' }] }, "$.values[?(@.id == '123')].value");

		expect(result).to.eq('test');
	});

	it('query of array in root', () => {
		const result = GetValueByJsonPath([{ id: '123', value: 'test' }], "$[?(@.id == '123')].value");

		expect(result).to.eq('test');
	});

	it('multi-AND filter matches all conditions, not just the first', () => {
		const data = [
			{ alias: 'blocks', culture: 'en-US', value: 'en' },
			{ alias: 'blocks', culture: 'bs', value: 'bs' },
		];

		const result = GetValueByJsonPath(data, `$[?(@.alias == 'blocks' && @.culture == 'bs')].value`);

		expect(result).to.eq('bs');
	});

	it('matches the null literal in a filter predicate', () => {
		const data = [
			{ alias: 'a', segment: 'foo', value: 'with-segment' },
			{ alias: 'a', segment: null, value: 'no-segment' },
		];

		const result = GetValueByJsonPath(data, `$[?(@.alias == 'a' && @.segment == null)].value`);

		expect(result).to.eq('no-segment');
	});
});
