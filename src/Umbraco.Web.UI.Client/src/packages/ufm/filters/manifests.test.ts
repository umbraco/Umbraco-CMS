import { expect } from '@open-wc/testing';
import { manifests } from './manifests.js';

describe('UFM Filter Manifests', () => {
	it('should export correct number of filter manifests', () => {
		// 8 original filters + 3 camelCase aliases = 11 total
		expect(manifests).to.have.lengthOf(11);
	});

	it('should register strip-html filter with hyphenated alias', () => {
		const filter = manifests.find((m) => m.meta.alias === 'strip-html');
		expect(filter).to.not.be.undefined;
		expect(filter?.alias).to.equal('Umb.Filter.StripHtml');
	});

	it('should register stripHtml filter with camelCase alias', () => {
		const filter = manifests.find((m) => m.meta.alias === 'stripHtml');
		expect(filter).to.not.be.undefined;
		expect(filter?.alias).to.equal('Umb.Filter.StripHtmlCamelCase');
	});

	it('should register title-case filter with hyphenated alias', () => {
		const filter = manifests.find((m) => m.meta.alias === 'title-case');
		expect(filter).to.not.be.undefined;
		expect(filter?.alias).to.equal('Umb.Filter.TitleCase');
	});

	it('should register titleCase filter with camelCase alias', () => {
		const filter = manifests.find((m) => m.meta.alias === 'titleCase');
		expect(filter).to.not.be.undefined;
		expect(filter?.alias).to.equal('Umb.Filter.TitleCaseCamelCase');
	});

	it('should register word-limit filter with hyphenated alias', () => {
		const filter = manifests.find((m) => m.meta.alias === 'word-limit');
		expect(filter).to.not.be.undefined;
		expect(filter?.alias).to.equal('Umb.Filter.WordLimit');
	});

	it('should register wordLimit filter with camelCase alias', () => {
		const filter = manifests.find((m) => m.meta.alias === 'wordLimit');
		expect(filter).to.not.be.undefined;
		expect(filter?.alias).to.equal('Umb.Filter.WordLimitCamelCase');
	});

	it('both strip-html and stripHtml should point to same implementation', () => {
		const hyphenated = manifests.find((m) => m.meta.alias === 'strip-html');
		const camelCase = manifests.find((m) => m.meta.alias === 'stripHtml');
		expect(hyphenated?.api).to.equal(camelCase?.api);
	});

	it('both title-case and titleCase should point to same implementation', () => {
		const hyphenated = manifests.find((m) => m.meta.alias === 'title-case');
		const camelCase = manifests.find((m) => m.meta.alias === 'titleCase');
		expect(hyphenated?.api).to.equal(camelCase?.api);
	});

	it('both word-limit and wordLimit should point to same implementation', () => {
		const hyphenated = manifests.find((m) => m.meta.alias === 'word-limit');
		const camelCase = manifests.find((m) => m.meta.alias === 'wordLimit');
		expect(hyphenated?.api).to.equal(camelCase?.api);
	});
});

