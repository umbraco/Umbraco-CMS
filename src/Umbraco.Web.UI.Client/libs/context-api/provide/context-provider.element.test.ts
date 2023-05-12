import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbContextConsumerController } from '../consume/context-consumer.controller';
import { UmbContextProviderElement } from './context-provider.element';
import { UmbControllerHostMixin } from '@umbraco-cms/backoffice/controller';

@customElement('umb-context-test')
export class UmbContextTestElement extends UmbControllerHostMixin(HTMLElement) {
	public value: string | null = null;
	constructor() {
		super();

		new UmbContextConsumerController<string>(this, 'test-context', (value) => {
			this.value = value;
		});
	}
}

describe('UmbContextProvider', () => {
	let element: UmbContextProviderElement;
	let consumer: UmbContextTestElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-context-provider key="test-context" .value=${contextValue}>
				<umb-context-test></umb-context-test>
			</umb-context-provider>`
		);
		consumer = element.getElementsByTagName('umb-context-test')[0] as UmbContextTestElement;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbContextProviderElement);
	});

	it('provides the context', () => {
		expect(consumer.value).to.equal(contextValue);
	});
});
