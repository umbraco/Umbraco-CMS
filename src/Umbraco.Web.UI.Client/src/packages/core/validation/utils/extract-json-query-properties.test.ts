import { expect } from '@open-wc/testing';
import { extractJsonQueryProps } from './extract-json-query-properties.function.js';

describe('UmbJsonPathFunctions', () => {
	it('retrieve property value', () => {
		const query = `?(@.culture == 'en-us' && @.segment == 'mySegment')`;
		const result = extractJsonQueryProps(query);

		expect(result.culture).to.eq('en-us');
		expect(result.segment).to.eq('mySegment');
	});
});
