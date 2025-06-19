import { expect } from '@open-wc/testing';
import { umbQueryMapperForJsonPaths } from './query-mapper-json-paths.function.js';
import { assert } from 'chai';

describe('umbQueryMapperForJsonPaths', () => {
	it('provides queries for index based pointers', async () => {
		const data = [
			{ key: '0', value: 'one' },
			{ key: '1', value: 'two' },
			{ key: '2', value: 'three' },
		];
		const paths = [`$[?(@.key == '0')].value`, `$[1].value`];

		const result = await umbQueryMapperForJsonPaths(paths, data, (entry: (typeof data)[0]) => {
			return `?(@.key == '${entry.key}')`;
		});

		expect(result.length).to.eq(2);
		expect(result[0]).to.eq(`$[?(@.key == '0')].value`);
		expect(result[1]).to.eq(`$[?(@.key == '1')].value`);
	});

	it('only handles the first index based pointers', async () => {
		const data = [
			{ key: '0', value: 'one' },
			{ key: '1', value: 'two' },
			{ key: '2', value: 'three' },
		];
		const paths = [`$[?(@.key == '0')].value`, `$[1].value.inner[1234].hello`];

		const result = await umbQueryMapperForJsonPaths(paths, data, (entry: (typeof data)[0]) => {
			return `?(@.key == '${entry.key}')`;
		});

		expect(result.length).to.eq(2);
		expect(result[0]).to.eq(`$[?(@.key == '0')].value`);
		expect(result[1]).to.eq(`$[?(@.key == '1')].value.inner[1234].hello`);
	});

	it('runs the mapper for both index and query based pointers', async () => {
		const data = [
			{ key: '0', value: 'one' },
			{ key: '1', value: 'two' },
			{ key: '2', value: 'three' },
		];
		const paths = [`$[?(@.key == '0')].value.hey`, `$[0].value`, `$[1].value`];

		let testCounter = 0;

		const result = await umbQueryMapperForJsonPaths(
			paths,
			data,
			(entry: (typeof data)[0]) => {
				return `?(@.key == '${entry.key}')`;
			},
			async (paths: Array<string>, data) => {
				testCounter++;
				if (testCounter === 1) {
					expect(paths.length).to.eq(2);
					expect(paths[0]).to.eq(`$.value.hey`);
					expect(paths[1]).to.eq(`$.value`);
					expect(data?.value).to.eq(`one`);
				} else if (testCounter === 2) {
					expect(paths.length).to.eq(1);
					expect(paths[0]).to.eq(`$.value`);
					expect(data?.value).to.eq(`two`);
				} else {
					assert.fail('The mapper should only run twice');
				}
				return paths;
			},
		);

		expect(result.length).to.eq(3);
		expect(result[0]).to.eq(`$[?(@.key == '0')].value.hey`);
		expect(result[1]).to.eq(`$[?(@.key == '0')].value`);
		expect(result[2]).to.eq(`$[?(@.key == '1')].value`);
	});
});
