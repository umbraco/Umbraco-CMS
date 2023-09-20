import { expect } from '@open-wc/testing';
import { extendDataSourceResponseData } from './extend-data-source-response-data.function.js';
import { DataSourceResponse } from './data-source-response.interface.js';

describe('extendDataSourceResponseData', () => {
	it('is a function', () => {
		expect(extendDataSourceResponseData).that.is.a('function');
	});

	describe('Extending data set', () => {
		it('has extended data of DataSourceResponse', () => {
			const response: DataSourceResponse<object> = {
				data: {
					original: 'prop',
				},
			};

			const extendedResponse = extendDataSourceResponseData(response, { foo: 'bar' });

			expect(extendedResponse.data).to.have.property('original').to.be.equal('prop');
			expect(extendedResponse.data).to.have.property('foo').to.be.equal('bar');
		});
	});
});
