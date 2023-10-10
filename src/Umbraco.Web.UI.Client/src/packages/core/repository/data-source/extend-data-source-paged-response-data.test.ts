import { expect } from '@open-wc/testing';
import { type UmbPagedData } from '../tree-repository.interface.js';
import { type DataSourceResponse } from './data-source-response.interface.js';
import { extendDataSourcePagedResponseData } from './extend-data-source-paged-response-data.function.js';

describe('extendDataSourcePagedResponseData', () => {
	it('is a function', () => {
		expect(extendDataSourcePagedResponseData).that.is.a('function');
	});

	describe('Extending data set', () => {
		it('has an controllerAlias property', () => {
			const response: DataSourceResponse<UmbPagedData<object>> = {
				data: {
					items: [
						{
							original: 'prop',
						},
						{
							original: 'prop',
						},
					],
					total: 2,
				},
			};

			const extendedResponse = extendDataSourcePagedResponseData(response, { foo: 'bar' });

			expect(extendedResponse.data).that.is.a('object');
			expect(extendedResponse.data?.items[1]).to.have.property('original').to.be.equal('prop');
			expect(extendedResponse.data?.items[1]).to.have.property('foo').to.be.equal('bar');
		});
	});
});
