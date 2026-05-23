import { expect } from '@open-wc/testing';
import { fromPascalCase } from './from-pascal-case.function.js';

describe('fromPascalCase', () => {
	it('splits PascalCase into words', () => {
		expect(fromPascalCase('ScheduledPublish')).to.equal('Scheduled Publish');
	});

	it('handles single word', () => {
		expect(fromPascalCase('Rollback')).to.equal('Rollback');
	});

	it('handles multiple transitions', () => {
		expect(fromPascalCase('PackageInstall')).to.equal('Package Install');
	});

	it('returns empty string for empty input', () => {
		expect(fromPascalCase('')).to.equal('');
	});
});
