import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostInitializerElement } from './controller-host-test.element';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement, UmbControllerHostMixin } from '@umbraco-cms/backoffice/controller';

@customElement('umb-controller-host-test-consumer')
export class UmbControllerHostTestConsumerElement extends UmbControllerHostMixin(HTMLElement) {
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
	let consumer: UmbControllerHostTestConsumerElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-controller-host-initializer
				.create=${(host: UmbControllerHostElement) =>
					new UmbContextProviderController(host, 'my-test-context-alias', contextValue)}>
				<umb-controller-host-test-consumer></umb-controller-host-test-consumer>
			</umb-controller-host-initializer>`
		);
		consumer = element.getElementsByTagName(
			'umb-controller-host-test-consumer'
		)[0] as UmbControllerHostTestConsumerElement;
	});

	it('element is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbControllerHostInitializerElement);
	});

	it('provides the context', () => {
		expect(consumer.value).to.equal(contextValue);
	});
});
