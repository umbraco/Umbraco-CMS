import { expect } from '@open-wc/testing';
import { umbParseValueListOptions } from './value-list-options.js';

describe('umbParseValueListOptions', () => {
	it('reads plain strings as values with the value as label', () => {
		expect(umbParseValueListOptions(['one', 'two'])).to.deep.equal([
			{ value: 'one', label: 'one' },
			{ value: 'two', label: 'two' },
		]);
	});

	it('reads objects with a value and a label', () => {
		expect(umbParseValueListOptions([{ value: 'one', label: 'One' }])).to.deep.equal([{ value: 'one', label: 'One' }]);
	});

	it('reads objects with a name as the label', () => {
		expect(umbParseValueListOptions([{ name: 'One', value: 'one' }])).to.deep.equal([{ value: 'one', label: 'One' }]);
	});

	it('leaves the label empty for an object without one', () => {
		expect(
			umbParseValueListOptions([{ value: 'one' }, { value: 'two', label: null }, { value: 'three', label: '' }]),
		).to.deep.equal([
			{ value: 'one', label: '' },
			{ value: 'two', label: '' },
			{ value: 'three', label: '' },
		]);
	});

	it('reads a mix of strings and objects in order', () => {
		expect(umbParseValueListOptions(['one', { value: 'two', label: 'Two' }, 'three'])).to.deep.equal([
			{ value: 'one', label: 'one' },
			{ value: 'two', label: 'Two' },
			{ value: 'three', label: 'three' },
		]);
	});

	it('skips entries without a value', () => {
		expect(umbParseValueListOptions(['one', { label: 'Two' }, null, 3])).to.deep.equal([
			{ value: 'one', label: 'one' },
		]);
	});

	it('returns no options when there are no items', () => {
		expect(umbParseValueListOptions(undefined)).to.deep.equal([]);
		expect(umbParseValueListOptions(null)).to.deep.equal([]);
		expect(umbParseValueListOptions('one')).to.deep.equal([]);
	});
});
