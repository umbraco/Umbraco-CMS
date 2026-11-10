import { expect } from '@open-wc/testing';
import {
	umbGetBlockedTransferVariants,
	umbResolveBlockTransferCultures,
	type UmbBlockTransferLanguageAccess,
} from './block-transfer-variants.function.js';

const access = (overrides: Partial<UmbBlockTransferLanguageAccess> = {}): UmbBlockTransferLanguageAccess => ({
	hasAccessToAllLanguages: false,
	languages: ['en-US'],
	hasAccessToInvariantForVariant: true,
	...overrides,
});

describe('umbGetBlockedTransferVariants', () => {
	it('blocks cultures the user has no access to', () => {
		const result = umbGetBlockedTransferVariants({
			valueCultures: ['en-US', 'da-DK', 'da-DK'],
			variesByCulture: true,
			access: access(),
		});

		expect(result).to.deep.equal({ cultures: ['da-DK'], invariant: false });
	});

	it('blocks nothing for a user with access to all languages', () => {
		const result = umbGetBlockedTransferVariants({
			valueCultures: ['en-US', 'da-DK'],
			variesByCulture: true,
			access: access({ hasAccessToAllLanguages: true, languages: [] }),
		});

		expect(result).to.deep.equal({ cultures: [], invariant: false });
	});

	it('blocks shared data of culture-varying content without the invariant permission', () => {
		const result = umbGetBlockedTransferVariants({
			valueCultures: ['en-US', null],
			variesByCulture: true,
			access: access({ hasAccessToInvariantForVariant: false }),
		});

		expect(result).to.deep.equal({ cultures: [], invariant: true });
	});

	it('does not block shared data when the content type does not vary by culture', () => {
		const result = umbGetBlockedTransferVariants({
			valueCultures: [null],
			variesByCulture: false,
			access: access({ hasAccessToInvariantForVariant: false }),
		});

		expect(result).to.deep.equal({ cultures: [], invariant: false });
	});
});

describe('umbResolveBlockTransferCultures', () => {
	it('creates a single invariant variant when the content type does not vary by culture', () => {
		const result = umbResolveBlockTransferCultures({
			valueCultures: [null],
			exposedCultures: [null],
			variesByCulture: false,
			fallbackCulture: 'en-US',
			access: access(),
		});

		expect(result).to.deep.equal({ cultures: [null], culturesToPublish: [null] });
	});

	it('does not publish an invariant block that is not exposed', () => {
		const result = umbResolveBlockTransferCultures({
			valueCultures: [null],
			exposedCultures: [],
			variesByCulture: false,
			fallbackCulture: undefined,
			access: access(),
		});

		expect(result).to.deep.equal({ cultures: [null], culturesToPublish: [] });
	});

	it('creates variants for value and exposed cultures and publishes the exposed ones', () => {
		const result = umbResolveBlockTransferCultures({
			valueCultures: ['en-US', 'da-DK', null],
			exposedCultures: ['en-US'],
			variesByCulture: true,
			fallbackCulture: undefined,
			access: access({ hasAccessToAllLanguages: true }),
		});

		expect(result).to.deep.equal({ cultures: ['en-US', 'da-DK'], culturesToPublish: ['en-US'] });
	});

	it('leaves out exposed cultures the user has no access to', () => {
		const result = umbResolveBlockTransferCultures({
			valueCultures: ['en-US'],
			exposedCultures: ['en-US', 'da-DK'],
			variesByCulture: true,
			fallbackCulture: undefined,
			access: access(),
		});

		expect(result).to.deep.equal({ cultures: ['en-US'], culturesToPublish: ['en-US'] });
	});

	it('falls back to the given culture when the block has no culture of its own', () => {
		const result = umbResolveBlockTransferCultures({
			valueCultures: [null],
			exposedCultures: [],
			variesByCulture: true,
			fallbackCulture: 'en-US',
			access: access(),
		});

		expect(result).to.deep.equal({ cultures: ['en-US'], culturesToPublish: [] });
	});
});
