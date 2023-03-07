import { expect, fixture, defineCE } from '@open-wc/testing';
import { UmbContextConsumer } from '../consume/context-consumer';
import { UmbContextProviderController } from './context-provider.controller';
import { UmbLitElement } from '@umbraco-cms/element';

class MyClass {
	prop = 'value from provider';
}

class ControllerHostElement extends UmbLitElement {}
const controllerHostElement = defineCE(ControllerHostElement);

describe('UmbContextProviderController', () => {
	let instance: MyClass;
	let provider: UmbContextProviderController;
	let element: UmbLitElement;

	beforeEach(async () => {
		element = await fixture(`<${controllerHostElement}></${controllerHostElement}>`);
		instance = new MyClass();
		provider = new UmbContextProviderController(element, 'my-test-context', instance);
	});

	describe('Public API', () => {
		describe('properties', () => {
			it('has a unique property', () => {
				expect(provider).to.have.property('unique');
			});
			it('has a unique property, is equal to the unique', () => {
				expect(provider.unique).to.eq('my-test-context');
			});
		});

		describe('methods', () => {
			it('has an providerInstance method', () => {
				expect(provider).to.have.property('providerInstance').that.is.a('function');
			});
		});
	});

	it('works with UmbContextConsumer', (done) => {
		const localConsumer = new UmbContextConsumer(element, 'my-test-context', (_instance: MyClass) => {
			expect(_instance.prop).to.eq('value from provider');
			done();
			localConsumer.hostDisconnected();
		});
		localConsumer.hostConnected();
	});

	it('Fails providing the same instance with another controller using the same unique', () => {
		let secondCtrl;

		// Tests that the creations throws:
		expect(() => {
			secondCtrl = new UmbContextProviderController(element, 'my-test-context', instance);
		}).to.throw();

		// Still has the initial controller:
		expect(element.hasController(provider)).to.be.true;
		// The secondCtrl was never set as a result of the creation failing:
		expect(secondCtrl).to.be.undefined;
	});
});
