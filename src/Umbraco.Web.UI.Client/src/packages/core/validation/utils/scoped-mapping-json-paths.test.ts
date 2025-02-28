import { expect } from '@open-wc/testing';
import { umbScopedMappingOfJsonPaths } from './scoped-mapping-json-paths.function.js';

describe('umbReduceAndScopeJsonPaths', () => {
	it('narrows paths', async () => {
		const paths = [
			'$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value',
			'$.array[2].value',
			'$.somethingelse[6].value',
		];
		const result = await umbScopedMappingOfJsonPaths(paths, '$.array', async (scopedPaths: Array<string>) => {
			expect(scopedPaths.length).to.eq(2);
			expect(scopedPaths[0]).to.eq('$[?(@.culture == "en-us" && @.segment == "mySegment")].value');
			expect(scopedPaths[1]).to.eq('$[2].value');
			return scopedPaths;
		});

		expect(result[0]).to.eq('$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value');
		expect(result[1]).to.eq('$.array[2].value');
		expect(result[2]).to.eq('$.somethingelse[6].value');
	});

	it('narrows translates the scoped paths', async () => {
		const paths = [
			'$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value',
			'$.array[2].value',
			'$.somethingelse[6].value',
		];
		const result = await umbScopedMappingOfJsonPaths(paths, '$.array[2]', async (scopedPaths: Array<string>) => {
			expect(scopedPaths.length).to.eq(1);
			expect(scopedPaths[0]).to.eq('$.value');
			return ['$.changedValue'];
		});

		expect(result[0]).to.eq('$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value');
		expect(result[1]).to.eq('$.array[2].changedValue');
		expect(result[2]).to.eq('$.somethingelse[6].value');
	});
});
