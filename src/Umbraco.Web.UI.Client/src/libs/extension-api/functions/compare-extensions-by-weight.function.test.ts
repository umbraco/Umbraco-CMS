import { compareExtensionsByWeight } from './compare-extensions-by-weight.function.js';
import { expect } from '@open-wc/testing';

describe('compareExtensionsByWeight', () => {
	it('sorts the highest weight first', () => {
		const sorted = [
			{ alias: 'a', weight: 1 },
			{ alias: 'b', weight: 100 },
			{ alias: 'c', weight: 10 },
		].sort(compareExtensionsByWeight);
		expect(sorted.map((x) => x.alias)).to.deep.equal(['b', 'c', 'a']);
	});

	it('sorts equal weights by alias, whatever the input order', () => {
		const sorted = [
			{ alias: 'Umb.C', weight: 10 },
			{ alias: 'Umb.A', weight: 10 },
			{ alias: 'Umb.B', weight: 10 },
		].sort(compareExtensionsByWeight);
		expect(sorted.map((x) => x.alias)).to.deep.equal(['Umb.A', 'Umb.B', 'Umb.C']);
	});

	it('treats a missing weight as 0', () => {
		const sorted = [{ alias: 'a' }, { alias: 'b', weight: -1 }, { alias: 'c', weight: 1 }].sort(
			compareExtensionsByWeight,
		);
		expect(sorted.map((x) => x.alias)).to.deep.equal(['c', 'a', 'b']);
	});
});
