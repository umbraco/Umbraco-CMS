import { expect } from '@open-wc/testing';
import { ReplaceStartOfPath } from './replace-start-of-path.function.js';

describe('ReplaceStartOfPath', () => {
	it('replaces a dot path', () => {
		const result = ReplaceStartOfPath('$.start.test', '$.start', '$');

		expect(result).to.eq('$.test');
	});

	it('replaces a array path', () => {
		const result = ReplaceStartOfPath('$.start[0].test', '$.start[0]', '$');

		expect(result).to.eq('$.test');
	});

	it('replaces a exact path', () => {
		const result = ReplaceStartOfPath('$.start.test', '$.start.test', '$');

		expect(result).to.eq('$');
	});
});
