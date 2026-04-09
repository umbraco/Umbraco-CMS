import { expect } from '@open-wc/testing';
import { getConfigValue } from './index.js';

describe('getConfigValue', () => {
	it('should return the value for a matching alias', () => {
		const config = [
			{ alias: 'foo', value: 123 },
			{ alias: 'bar', value: 'hello' },
		];
		const result = getConfigValue(config, 'foo');
		expect(result).to.equal(123);
	});

	it('should return undefined if alias is not found', () => {
		const config = [
			{ alias: 'foo', value: 123 },
			{ alias: 'bar', value: 'hello' },
		];
		const result = getConfigValue(config, 'baz');
		expect(result).to.be.undefined;
	});

	it('should return undefined if config is undefined', () => {
		const result = getConfigValue(undefined, 'foo');
		expect(result).to.be.undefined;
	});

	it('should work with different value types', () => {
		const config = [
			{ alias: 'num', value: 42 },
			{ alias: 'str', value: 'test' },
			{ alias: 'obj', value: { a: 1 } },
		];
		expect(getConfigValue(config, 'num')).to.equal(42);
		expect(getConfigValue(config, 'str')).to.equal('test');
		expect(getConfigValue(config, 'obj')).to.deep.equal({ a: 1 });
	});
});
