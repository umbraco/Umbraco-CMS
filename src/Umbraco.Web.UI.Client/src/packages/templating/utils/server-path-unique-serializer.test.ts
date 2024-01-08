import { expect } from '@open-wc/testing';
import { UmbServerPathUniqueSerializer } from './server-path-unique-serializer.js';

describe('UmbServerPathUniqueSerializer', () => {
	let serializer: UmbServerPathUniqueSerializer;
	const serverPath = 'Folder A/Folder AA/some-Filename-WithCasing_underscore.js';
	const expectedUnique = 'Folder%20A%2FFolder%20AA%2Fsome-Filename-WithCasing_underscore%25dot%25js';

	beforeEach(() => {
		serializer = new UmbServerPathUniqueSerializer();
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a toUnique method', () => {
				expect(serializer).to.have.property('toUnique').that.is.a('function');
			});

			it('has a toParentUnique method', () => {
				expect(serializer).to.have.property('toParentUnique').that.is.a('function');
			});

			it('has a toServerPath method', () => {
				expect(serializer).to.have.property('toServerPath').that.is.a('function');
			});
		});
	});

	describe('toUnique', () => {
		it('converts a server path to a URL friendly unique', () => {
			expect(serializer.toUnique(serverPath)).to.equal(expectedUnique);
		});
	});

	describe('toServerPath', () => {
		it('converts a URL friendly unique to a server path', () => {
			expect(serializer.toServerPath(expectedUnique)).to.equal(serverPath);
		});
	});
});
