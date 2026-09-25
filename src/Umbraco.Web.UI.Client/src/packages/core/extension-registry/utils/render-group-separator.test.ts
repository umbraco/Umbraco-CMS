import { umbRenderGroupSeparator } from './render-group-separator.function.js';
import { expect } from '@open-wc/testing';
import { nothing } from '@umbraco-cms/backoffice/external/lit';

describe('umbRenderGroupSeparator', () => {
	it('renders nothing before the first item', () => {
		expect(umbRenderGroupSeparator(undefined, { group: 'a' })).to.equal(nothing);
	});

	it('renders nothing between items of the same group', () => {
		expect(umbRenderGroupSeparator({ group: 'a' }, { group: 'a' })).to.equal(nothing);
	});

	it('renders nothing between two items without a group', () => {
		expect(umbRenderGroupSeparator({}, {})).to.equal(nothing);
	});

	it('renders a separator between items of different groups', () => {
		expect(umbRenderGroupSeparator({ group: 'a' }, { group: 'b' })).to.not.equal(nothing);
	});

	it('renders a separator between a grouped and an ungrouped item', () => {
		expect(umbRenderGroupSeparator({ group: 'a' }, {})).to.not.equal(nothing);
		expect(umbRenderGroupSeparator({}, { group: 'a' })).to.not.equal(nothing);
	});
});
