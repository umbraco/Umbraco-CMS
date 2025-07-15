import { expect } from '@open-wc/testing';
import { umbScopeMapperForJsonPaths } from './scope-mapper-json-paths.function.js';

describe('umbScopeMapperForJsonPaths', () => {
	it('narrows paths', async () => {
		const paths = [
			'$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value',
			'$.array[2].value',
			'$.array[3].value.innerArray[0].value',
			'$.somethingelse[6].value',
		];
		const result = await umbScopeMapperForJsonPaths(paths, '$.array', async (scopedPaths: Array<string>) => {
			expect(scopedPaths.length).to.eq(3);
			expect(scopedPaths[0]).to.eq('$[?(@.culture == "en-us" && @.segment == "mySegment")].value');
			expect(scopedPaths[1]).to.eq('$[2].value');
			expect(scopedPaths[2]).to.eq('$[3].value.innerArray[0].value');
			return scopedPaths;
		});

		expect(result[0]).to.eq('$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value');
		expect(result[1]).to.eq('$.array[2].value');
		expect(result[2]).to.eq('$.array[3].value.innerArray[0].value');
		expect(result[3]).to.eq('$.somethingelse[6].value');
	});

	it('narrows translates the scoped paths', async () => {
		const paths = [
			'$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value',
			'$.array[2].value',
			'$.somethingelse[6].value',
		];
		const result = await umbScopeMapperForJsonPaths(paths, '$.array[2]', async (scopedPaths: Array<string>) => {
			expect(scopedPaths.length).to.eq(1);
			expect(scopedPaths[0]).to.eq('$.value');
			return ['$.changedValue'];
		});

		expect(result[0]).to.eq('$.array[?(@.culture == "en-us" && @.segment == "mySegment")].value');
		expect(result[1]).to.eq('$.array[2].changedValue');
		expect(result[2]).to.eq('$.somethingelse[6].value');
	});
});
