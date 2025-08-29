import { expect } from '@open-wc/testing';
import { linkEntityExpansionEntries } from './link-entity-expansion-entries';

describe('LinkEntityExpansionEntries', () => {
	const input = [
		{ entityType: 'document', unique: '1' },
		{ entityType: 'document', unique: '2' },
		{ entityType: 'document', unique: '3' },
	];

	it('should return an array of expansion entries with target', () => {
		const result = linkEntityExpansionEntries(input);
		expect(result).to.be.an('array').that.has.lengthOf(3);
		expect(result[0]).to.have.property('entityType', 'document');
		expect(result[0]).to.have.property('unique', '1');
		expect(result[0]).to.have.property('target').that.deep.equals({ entityType: 'document', unique: '2' });

		expect(result[1]).to.have.property('entityType', 'document');
		expect(result[1]).to.have.property('unique', '2');
		expect(result[1]).to.have.property('target').that.deep.equals({ entityType: 'document', unique: '3' });

		expect(result[2]).to.have.property('entityType', 'document');
		expect(result[2]).to.have.property('unique', '3');
		expect(result[2]).to.not.have.property('target');
	});
});
