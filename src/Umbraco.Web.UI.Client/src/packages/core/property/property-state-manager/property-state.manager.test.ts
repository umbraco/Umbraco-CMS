import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyStateManager } from './property-state.manager.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbPropertyStateManager', () => {
	let manager: UmbPropertyStateManager;
	const propA = { unique: 'propA' };
	const propB = { unique: 'propB' };
	const statePropA = { unique: '1', message: 'State 1', state: true, propertyType: propA };
	const statePropB = { unique: '2', message: 'State 2', state: true, propertyType: propB };
	const statePlain = { unique: '3', message: 'State 3', state: true };
	const stateNoPropA = { unique: '01', message: 'State 01', state: false, propertyType: propA };
	const stateNoPropB = { unique: '02', message: 'State 02', state: false, propertyType: propB };
	const stateNoPlain = { unique: '03', message: 'State 03', state: false };

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		manager = new UmbPropertyStateManager(hostElement);
	});

	describe('propertyTypes based states', () => {
		it('works with propertyTypes in the state data.', () => {
			manager.addState(statePropA);
			manager.addState(statePropB);
			expect(manager.getStates()[0].propertyType?.unique).to.be.equal(propA.unique);
			expect(manager.getStates()[1].propertyType?.unique).to.be.equal(propB.unique);
		});

		it('is not generally on when no states', () => {
			expect(manager.getIsOn()).to.be.false;
		});

		it('is generally on when states', () => {
			manager.addState(statePropA);
			expect(manager.getIsOn()).to.be.true;
		});

		it('is not on for a variant when no states', (done) => {
			manager
				.isOnForProperty(propA)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on for present variant', (done) => {
			manager.addState(statePropB);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on for incompatible variant', (done) => {
			manager.addState(statePropA);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is on by generic state', (done) => {
			manager.addState(statePlain);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific state states false', (done) => {
			manager.addState(statePlain);
			manager.addState(stateNoPropB);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when generic state states false', (done) => {
			manager.addState(stateNoPlain);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('is not on when specific state states true', (done) => {
			manager.addState(stateNoPlain);
			manager.addState(statePropB);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.true;
					done();
				})
				.unsubscribe();
		});

		it('a negative specific state wins', (done) => {
			manager.addState(stateNoPlain);
			manager.addState(statePropB);
			manager.addState(stateNoPropB);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});

		it('a negative general state wins', (done) => {
			manager.addState(stateNoPlain);
			manager.addState(statePlain);

			manager
				.isOnForProperty(propB)
				.subscribe((value) => {
					expect(value).to.be.false;
					done();
				})
				.unsubscribe();
		});
	});
});
