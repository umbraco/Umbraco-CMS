import { UmbVariantNameWriteGuardManager } from './variant-name-write-guard.manager.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('test-variant-name-write-guard-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbVariantNameWriteGuardManager', () => {
	let manager: UmbVariantNameWriteGuardManager;
	const enUS = new UmbVariantId('en-US');
	const daDK = new UmbVariantId('da-DK');
	const enUSSegment = new UmbVariantId('en-US', 'segment');

	beforeEach(() => {
		manager = new UmbVariantNameWriteGuardManager(new UmbTestControllerHostElement());
	});

	describe('without a variant', () => {
		it('is not permitted when there are no rules and the fallback is not permitted', () => {
			expect(manager.getIsPermittedForName()).to.be.false;
		});

		it('is permitted when there are no rules and the fallback is permitted', () => {
			manager.fallbackToPermitted();

			expect(manager.getIsPermittedForName()).to.be.true;
		});

		it('is permitted by a positive rule that is not bound to a variant', () => {
			manager.addRule({ unique: 'allow', permitted: true });

			expect(manager.getIsPermittedForName()).to.be.true;
		});

		it('is denied by a negative rule that is not bound to a variant, even next to a positive one', () => {
			manager.addRules([
				{ unique: 'allow', permitted: true },
				{ unique: 'deny', permitted: false },
			]);

			expect(manager.getIsPermittedForName()).to.be.false;
		});

		it('ignores rules that are bound to a variant', () => {
			manager.fallbackToPermitted();
			manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });

			expect(manager.getIsPermittedForName()).to.be.true;
		});
	});

	describe('for a variant', () => {
		it('resolves to the fallback when no rule applies', () => {
			expect(manager.getIsPermittedForName(enUS)).to.be.false;
			manager.fallbackToPermitted();
			expect(manager.getIsPermittedForName(enUS)).to.be.true;
		});

		it('only applies a rule that is bound to a variant to that variant', () => {
			manager.fallbackToPermitted();
			manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });

			expect(manager.getIsPermittedForName(enUS)).to.be.false;
			expect(manager.getIsPermittedForName(daDK)).to.be.true;
		});

		it('applies a rule that is not bound to a variant to every variant', () => {
			manager.fallbackToPermitted();
			manager.addRule({ unique: 'deny-all', permitted: false });

			expect(manager.getIsPermittedForName(enUS)).to.be.false;
			expect(manager.getIsPermittedForName(daDK)).to.be.false;
		});

		it('permits a variant through a positive rule when the fallback is not permitted', () => {
			manager.addRule({ unique: 'allow-da', variantId: daDK, permitted: true });

			expect(manager.getIsPermittedForName(daDK)).to.be.true;
			expect(manager.getIsPermittedForName(enUS)).to.be.false;
		});

		it('lets a negative rule take precedence over a positive rule for the same variant', () => {
			manager.addRules([
				{ unique: 'allow-en', variantId: enUS, permitted: true },
				{ unique: 'deny-en', variantId: enUS, permitted: false },
			]);

			expect(manager.getIsPermittedForName(enUS)).to.be.false;
		});

		it('lets a negative variant rule take precedence over a positive rule that is not bound to a variant', () => {
			manager.addRules([
				{ unique: 'allow-all', permitted: true },
				{ unique: 'deny-en', variantId: enUS, permitted: false },
			]);

			expect(manager.getIsPermittedForName(enUS)).to.be.false;
			expect(manager.getIsPermittedForName(daDK)).to.be.true;
		});

		it('matches the culture and the segment of the variant', () => {
			manager.fallbackToPermitted();
			manager.addRule({ unique: 'deny-en-segment', variantId: enUSSegment, permitted: false });

			expect(manager.getIsPermittedForName(enUSSegment)).to.be.false;
			expect(manager.getIsPermittedForName(enUS)).to.be.true;
		});

		it('permits the variant again when its rule is removed', () => {
			manager.fallbackToPermitted();
			manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });
			expect(manager.getIsPermittedForName(enUS)).to.be.false;

			manager.removeRule('deny-en');

			expect(manager.getIsPermittedForName(enUS)).to.be.true;
		});
	});

	describe('observing', () => {
		it('emits the current outcome and updates it when rules change', () => {
			manager.fallbackToPermitted();
			const emitted: Array<boolean> = [];
			const subscription = manager.isPermittedForName(enUS).subscribe((value) => emitted.push(value));

			manager.addRule({ unique: 'deny-en', variantId: enUS, permitted: false });
			manager.removeRule('deny-en');
			subscription.unsubscribe();

			expect(emitted).to.deep.equal([true, false, true]);
		});

		it('does not emit for rules that apply to another variant', () => {
			manager.fallbackToPermitted();
			const emitted: Array<boolean> = [];
			const subscription = manager.isPermittedForName(enUS).subscribe((value) => emitted.push(value));

			manager.addRule({ unique: 'deny-da', variantId: daDK, permitted: false });
			subscription.unsubscribe();

			expect(emitted).to.deep.equal([true]);
		});

		it('answers like the name guard when no variant is given', () => {
			const emitted: Array<boolean> = [];
			const subscription = manager.isPermittedForName().subscribe((value) => emitted.push(value));

			manager.addRule({ unique: 'allow', permitted: true });
			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true]);
		});
	});
});
