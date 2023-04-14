import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostTestElement } from './controller-host-test.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

@customElement('umb-controller-host-test-consumer')
export class ControllerHostTestConsumerElement extends UmbLitElement {
	public value: string | null = null;
	constructor() {
		super();
		this.consumeContext<string>('my-test-context-alias', (value) => {
			this.value = value;
		});
	}
}

describe('UmbControllerHostTestElement', () => {
	let element: UmbControllerHostTestElement;
	let consumer: ControllerHostTestConsumerElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-controller-host-test
				.create=${(host: UmbControllerHostElement) =>
					new UmbContextProviderController(host, 'my-test-context-alias', contextValue)}>
				<umb-controller-host-test-consumer></umb-controller-host-test-consumer>
			</umb-controller-host-test>`
		);
		consumer = element.getElementsByTagName(
			'umb-controller-host-test-consumer'
		)[0] as ControllerHostTestConsumerElement;
	});

	it('element is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbControllerHostTestElement);
	});

	it('provides the context', () => {
		expect(consumer.value).to.equal(contextValue);
	});
});
