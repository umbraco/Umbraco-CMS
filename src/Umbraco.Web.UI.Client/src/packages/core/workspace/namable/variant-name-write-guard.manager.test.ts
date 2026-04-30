import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantNameWriteGuardManager } from './variant-name-write-guard.manager.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

@customElement('test-variant-name-write-guard-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbVariantNameWriteGuardManager', () => {
	let manager: UmbVariantNameWriteGuardManager;

	const invariantVariant = UmbVariantId.CreateInvariant();
	const englishVariant = UmbVariantId.Create({ culture: 'en', segment: null });
	const danishVariant = UmbVariantId.Create({ culture: 'da', segment: null });

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbVariantNameWriteGuardManager(hostElement);
	});

	it('is not permitted when no rules (fallback is false)', (done) => {
		manager
			.isPermittedForVariantName(englishVariant)
			.subscribe((value) => {
				expect(value).to.be.false;
				done();
			})
			.unsubscribe();
	});

	it('is permitted for a matching variant rule', (done) => {
		manager.addRule({ unique: '1', message: 'permit en', permitted: true, variantId: englishVariant });

		manager
			.isPermittedForVariantName(englishVariant)
			.subscribe((value) => {
				expect(value).to.be.true;
				done();
			})
			.unsubscribe();
	});

	it('is not permitted for a non-matching variant rule', (done) => {
		manager.addRule({ unique: '1', message: 'permit en', permitted: true, variantId: englishVariant });

		manager
			.isPermittedForVariantName(danishVariant)
			.subscribe((value) => {
				expect(value).to.be.false;
				done();
			})
			.unsubscribe();
	});

	it('is permitted by a generic rule (no variantId)', (done) => {
		manager.addRule({ unique: '1', message: 'permit all', permitted: true });

		manager
			.isPermittedForVariantName(englishVariant)
			.subscribe((value) => {
				expect(value).to.be.true;
				done();
			})
			.unsubscribe();
	});

	it('deny rule takes precedence over permit rule', (done) => {
		manager.addRule({ unique: '1', message: 'permit all', permitted: true });
		manager.addRule({ unique: '2', message: 'deny en', permitted: false, variantId: englishVariant });

		manager
			.isPermittedForVariantName(englishVariant)
			.subscribe((value) => {
				expect(value).to.be.false;
				done();
			})
			.unsubscribe();
	});

	it('deny rule for one variant does not affect another', (done) => {
		manager.addRule({ unique: '1', message: 'permit all', permitted: true });
		manager.addRule({ unique: '2', message: 'deny en', permitted: false, variantId: englishVariant });

		manager
			.isPermittedForVariantName(danishVariant)
			.subscribe((value) => {
				expect(value).to.be.true;
				done();
			})
			.unsubscribe();
	});

	it('invariant variant is permitted by generic rule', (done) => {
		manager.addRule({ unique: '1', message: 'permit all', permitted: true });

		manager
			.isPermittedForVariantName(invariantVariant)
			.subscribe((value) => {
				expect(value).to.be.true;
				done();
			})
			.unsubscribe();
	});

	it('getIsPermittedForVariantName returns synchronous result', () => {
		manager.addRule({ unique: '1', message: 'permit en', permitted: true, variantId: englishVariant });
		expect(manager.getIsPermittedForVariantName(englishVariant)).to.be.true;
		expect(manager.getIsPermittedForVariantName(danishVariant)).to.be.false;
	});
});
