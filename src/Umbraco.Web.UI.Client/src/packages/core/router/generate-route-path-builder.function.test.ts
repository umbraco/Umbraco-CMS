import { umbGenerateRoutePathBuilder } from './generate-route-path-builder.function.js';
import { expect } from '@open-wc/testing';

describe('createRoutePathBuilder', () => {
	it('should return a function that builds a route path without parameters', () => {
		const buildPath = umbGenerateRoutePathBuilder('test/path');
		expect(buildPath(null)).to.eq('/test/path/');
	});

	it('should return a function that builds a route path with parameters', () => {
		const buildPath = umbGenerateRoutePathBuilder(':param0/test/:param1/path/:param2');
		expect(buildPath({ param0: 'value0', param1: 'value1', param2: 'value2' })).to.eq(
			'/value0/test/value1/path/value2/',
		);
	});

	it('should convert number parameters to strings', () => {
		const buildPath = umbGenerateRoutePathBuilder('test/:param1/path/:param2');
		expect(buildPath({ param1: 123, param2: 456 })).to.eq('/test/123/path/456/');
	});

	it('should not consider route segments that resembles parameters as parameters', () => {
		const buildPath = umbGenerateRoutePathBuilder('test/uc:store/path');
		expect(buildPath({ someOtherParam: 'test' })).to.eq('/test/uc:store/path/');
	});

	it('should support multiple parameters with the same name', () => {
		const buildPath = umbGenerateRoutePathBuilder('test/:param1/path/:param1');
		expect(buildPath({ param1: 'value1' })).to.eq('/test/value1/path/value1/');
	});

	it('should not consider parameters that are not in the params object', () => {
		const buildPath = umbGenerateRoutePathBuilder('test/:param1/path/:param2');
		expect(buildPath({ param1: 'value1' })).to.eq('/test/value1/path/:param2/');
	});

	it('should support complex objects as parameters with a custom toString method', () => {
		const buildPath = umbGenerateRoutePathBuilder('test/:param1/path/:param2');
		const obj = {
			toString() {
				return 'value1';
			},
		};
		expect(buildPath({ param1: obj, param2: 'value2' })).to.eq('/test/value1/path/value2/');
	});
});
