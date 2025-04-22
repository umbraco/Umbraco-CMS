import { UmbContextConsumerController } from '../consume/context-consumer.controller.js';
import { UmbContextProviderElement } from './context-provider.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

class UmbTestContextProviderControllerClass {
	prop = 'value from provider';
	getHostElement() {
		return undefined as unknown as Element;
	}
}

@customElement('umb-test-context')
export class UmbTestContextElement extends UmbControllerHostElementMixin(HTMLElement) {
	public value?: string;
	constructor() {
		super();

		new UmbContextConsumerController<UmbTestContextProviderControllerClass>(this, 'test-context', (context) => {
			this.value = context?.prop;
		});
	}
}

describe('UmbContextProvider', () => {
	let element: HTMLElement;
	let consumer: UmbTestContextElement;
	const context = new UmbTestContextProviderControllerClass();

	beforeEach(async () => {
		element = await fixture(
			html` <umb-context-provider key="test-context" .value=${context}>
				<umb-test-context></umb-test-context>
			</umb-context-provider>`,
		);
		consumer = element.getElementsByTagName('umb-test-context')[0] as unknown as UmbTestContextElement;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbContextProviderElement);
	});

	it('provides the context', () => {
		expect(consumer).to.equal(context);
		expect(consumer.value).to.equal('value from provider');
	});
});
