import { umbDeepMerge } from './deep-merge.function.js';
import { expect } from '@open-wc/testing';

describe('UmbDeepMerge', () => {
	beforeEach(() => {});

	describe('merge just objects', () => {
		it('transfers defined properties', () => {
			const defaults = {
				prop1: {
					name: 'prop1',
					value: 'value1',
				},
				prop2: {
					name: 'prop2',
					value: 'value2',
				},
			};
			const source = {
				prop2: {
					name: 'prop2_updatedName',
				},
			};
			const result = umbDeepMerge(source, defaults);

			expect(result.prop1.name).to.equal('prop1');
			expect(result.prop2.name).to.equal('prop2_updatedName');
		});
	});

	describe('merge objects with arrays', () => {
		// The arrays should not be merged, but take the value from the main object. [NL]
		it('transfers defined properties', () => {
			const defaults = {
				prop1: {
					name: 'prop1',
					value: ['entry1', 'entry2', 'entry3'],
				},
				prop2: {
					name: 'prop2',
					value: ['entry4', 'entry4', 'entry5'],
				},
			};
			const source = {
				prop1: {
					name: 'prop1_updatedName',
				},
				prop2: {
					name: 'prop2_updatedName',
					value: ['entry666'],
				},
			};
			const result = umbDeepMerge(source, defaults);

			expect(result.prop1.name).to.equal('prop1_updatedName');
			expect(result.prop1.value.join(',')).to.equal(defaults.prop1.value.join(','));
			expect(result.prop2.name).to.equal('prop2_updatedName');
			expect(result.prop2.value.join(',')).to.equal(source.prop2.value.join(','));
		});
	});
});
