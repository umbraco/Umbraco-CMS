import { UmbVariantGuardManager } from './variant-guard.manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('test-variant-guard-manager-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbVariantGuardManager', () => {
	let manager: UmbVariantGuardManager;
	const enUS = new UmbVariantId('en-US');
	const daDK = new UmbVariantId('da-DK');

	beforeEach(() => {
		manager = new UmbVariantGuardManager(new UmbTestControllerHostElement());
	});

	it('resolves to the fallback when no rule applies', () => {
		expect(manager.getIsPermittedForVariant(enUS)).to.be.false;
		manager.fallbackToPermitted();
		expect(manager.getIsPermittedForVariant(enUS)).to.be.true;
	});

	it('only applies a variant rule to the matching variant', () => {
		manager.fallbackToPermitted();
		manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });

		expect(manager.getIsPermittedForVariant(enUS)).to.be.false;
		expect(manager.getIsPermittedForVariant(daDK)).to.be.true;
	});

	it('applies a rule without a variantId to all variants', () => {
		manager.fallbackToPermitted();
		manager.addRule({ unique: 'deny-all', permitted: false });

		expect(manager.getIsPermittedForVariant(enUS)).to.be.false;
		expect(manager.getIsPermittedForVariant(daDK)).to.be.false;
	});

	it('lets a denying rule take precedence over a permitting rule', () => {
		manager.addRule({ unique: 'permit-en', variantId: enUS, permitted: true });
		manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });

		expect(manager.getIsPermittedForVariant(enUS)).to.be.false;
	});

	it('emits the permission for a variant', (done) => {
		manager.fallbackToPermitted();
		manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });

		manager
			.isPermittedForVariant(enUS)
			.subscribe((value) => {
				expect(value).to.be.false;
				done();
			})
			.unsubscribe();
	});
});
