import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostInitializerElement } from './controller-host-initializer.element';
import { UmbControllerHostElement, UmbControllerHostMixin } from './controller-host.mixin';
import { UmbContextConsumerController, UmbContextProviderController } from 'src/libs/context-api';

@customElement('umb-test-controller-host-initializer-consumer')
export class UmbTestControllerHostInitializerConsumerElement extends UmbControllerHostMixin(HTMLElement) {
	public value: string | null = null;
	constructor() {
		super();

		new UmbContextConsumerController<string>(this, 'my-test-context-alias', (value) => {
			this.value = value;
		});
	}
}

describe('UmbControllerHostTestElement', () => {
	let element: UmbControllerHostInitializerElement;
	let consumer: UmbTestControllerHostInitializerConsumerElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-controller-host-initializer
				.create=${(host: UmbControllerHostElement) =>
					new UmbContextProviderController(host, 'my-test-context-alias', contextValue)}>
				<umb-test-controller-host-initializer-consumer></umb-test-controller-host-initializer-consumer>
			</umb-controller-host-initializer>`
		);
		consumer = element.getElementsByTagName(
			'umb-test-controller-host-initializer-consumer'
		)[0] as UmbTestControllerHostInitializerConsumerElement;
	});

	it('element is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbControllerHostInitializerElement);
	});

	it('provides the context', () => {
		expect(consumer.value).to.equal(contextValue);
	});
});
