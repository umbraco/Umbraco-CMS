import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextConsumerController } from '../consume/context-consumer.controller.js';
import { UmbContextProviderElement } from './context-provider.element.js';
import { UmbControllerHostMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('umb-test-context')
export class UmbTestContextElement extends UmbControllerHostMixin(HTMLElement) {
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
	let consumer: UmbTestContextElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-context-provider key="test-context" .value=${contextValue}>
				<umb-test-context></umb-test-context>
			</umb-context-provider>`
		);
		consumer = element.getElementsByTagName('umb-test-context')[0] as UmbTestContextElement;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbContextProviderElement);
	});

	it('provides the context', () => {
		expect(consumer.value).to.equal(contextValue);
	});
});
