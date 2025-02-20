import { expect } from '@open-wc/testing';
import { umbObjectToPropertyValueArray } from './object-to-property-value-array.function.js';

describe('umbObjectToPropertyValueArray', () => {
	it('should return undefined if the data is undefined', () => {
		expect(umbObjectToPropertyValueArray(undefined)).to.be.undefined;
	});

	it('should return an array of property values', () => {
		const data = {
			key1: 'value1',
			key2: 'value2',
			key3: 'value3',
		};
		const result = umbObjectToPropertyValueArray(data);
		expect(result).to.have.length(3);
		expect(result?.[0]).to.deep.equal({ alias: 'key1', value: 'value1' });
		expect(result?.[1]).to.deep.equal({ alias: 'key2', value: 'value2' });
		expect(result?.[2]).to.deep.equal({ alias: 'key3', value: 'value3' });
		expect(result?.[3]).to.be.undefined;
	});
});
