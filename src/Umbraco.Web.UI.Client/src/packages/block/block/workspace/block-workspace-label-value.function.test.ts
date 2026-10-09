import { expect } from '@open-wc/testing';
import { buildBlockLabelValueObject } from './block-workspace-label-value.function.js';
import type { UmbBlockDataValueModel } from '../types.js';

function value(alias: string, val: unknown): UmbBlockDataValueModel {
	return { alias, value: val, culture: null, segment: null, editorAlias: 'test' };
}

describe('buildBlockLabelValueObject', () => {
	it('places content values at the top level keyed by alias', () => {
		const result = buildBlockLabelValueObject([value('heading', 'Hello')], undefined);
		expect(result.heading).to.equal('Hello');
	});

	it('places settings values under $settings as a key-value object keyed by alias', () => {
		const result = buildBlockLabelValueObject(undefined, [value('notes', 'A note')]);
		// $settings must be an object (not the raw value array) so labels can use `${$settings.notes}`. [NL]
		expect(result.$settings).to.eql({ notes: 'A note' });
	});

	it('includes $index when an index is provided', () => {
		const result = buildBlockLabelValueObject(undefined, undefined, 3);
		expect(result.$index).to.equal(3);
	});

	it('omits $index when no index is provided', () => {
		const result = buildBlockLabelValueObject(undefined, undefined);
		expect('$index' in result).to.be.false;
	});

	it('returns an empty object when no values are provided', () => {
		expect(buildBlockLabelValueObject(undefined, undefined)).to.eql({});
	});
});
