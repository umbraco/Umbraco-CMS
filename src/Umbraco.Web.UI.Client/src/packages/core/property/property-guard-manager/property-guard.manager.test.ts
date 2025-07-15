import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyGuardManager } from './property-guard.manager.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbPropertyGuardManager', () => {
	let manager: UmbPropertyGuardManager;
	const propA = { unique: 'propA' };
	const propB = { unique: 'propB' };
	const rulePropA = { unique: '1', message: 'State 1', permitted: true, propertyType: propA };
	const rulePropB = { unique: '2', message: 'State 2', permitted: true, propertyType: propB };
	const rulePlain = { unique: '3', message: 'State 3', permitted: true };
	const ruleNoPropA = { unique: '01', message: 'State 01', permitted: false, propertyType: propA };
	const ruleNoPropB = { unique: '02', message: 'State 02', permitted: false, propertyType: propB };
	const ruleNoPlain = { unique: '03', message: 'State 03', permitted: false };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbPropertyGuardManager(hostElement);
	});

	describe('propertyTypes based states', () => {
		it('works with propertyTypes in the state data.', () => {
			manager.addRule(rulePropA);
			manager.addRule(rulePropB);
			expect(manager.getRules()[0].propertyType?.unique).to.be.equal(propA.unique);
			expect(manager.getRules()[1].propertyType?.unique).to.be.equal(propB.unique);
		});

		it('is not permitted for a variant when no states', (done) => {
			manager
				.isPermittedForProperty(propA)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted for present variant', (done) => {
			manager.addRule(rulePropB);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted for incompatible variant', (done) => {
			manager.addRule(rulePropA);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is permitted by generic state', (done) => {
			manager.addRule(rulePlain);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted when specific state states false', (done) => {
			manager.addRule(rulePlain);
			manager.addRule(ruleNoPropB);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted when generic state states false', (done) => {
			manager.addRule(ruleNoPlain);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not permitted despite specific state states true', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(rulePropB);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific state wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(rulePropB);
			manager.addRule(ruleNoPropB);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative general state wins', (done) => {
			manager.addRule(ruleNoPlain);
			manager.addRule(rulePlain);

			manager
				.isPermittedForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
