import { expect, fixture, html } from '@open-wc/testing';
import { LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from './context-consumer.mixin';
import { UmbContextProviderElement } from './context-provider.element';

@customElement('umb-context-test')
export class ContextTestElement extends UmbContextConsumerMixin(LitElement) {
	public value: string | null = null;
	constructor() {
		super();
		this.consumeContext('test-context', (value) => {
			this.value = value;
		});
	}
}

describe('UmbContextProvider', () => {
	let element: UmbContextProviderElement;
	let consumer: ContextTestElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-context-provider key="test-context" .value=${contextValue}>
				<umb-context-test></umb-context-test>
			</umb-context-provider>`
		);
		consumer = element.getElementsByTagName('umb-context-test')[0] as ContextTestElement;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbContextProviderElement);
	});

	it('provides the context', () => {
		expect(consumer.value).to.equal(contextValue);
	});
});
