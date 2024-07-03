import { UmbControllerHostProviderElement } from './controller-host-provider.element.js';
import { UmbControllerHostElementMixin } from './controller-host-element.mixin.js';
import type { UmbControllerHostElement } from './controller-host-element.interface.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-test-controller-host-initializer-consumer')
export class UmbTestControllerHostInitializerConsumerElement extends UmbControllerHostElementMixin(HTMLElement) {
	public value: string | null = null;
	constructor() {
		super();

		new UmbContextConsumerController<string>(this, 'my-test-context-alias', (value) => {
			this.value = value;
		});
	}
}

describe('UmbControllerHostTestElement', () => {
	let element: UmbControllerHostProviderElement;
	let consumer: UmbTestControllerHostInitializerConsumerElement;
	const contextValue = 'test-value';

	beforeEach(async () => {
		element = await fixture(
			html` <umb-controller-host-provider
				.create=${(host: UmbControllerHostElement) =>
					new UmbContextProviderController(host, 'my-test-context-alias', contextValue)}>
				<umb-test-controller-host-initializer-consumer></umb-test-controller-host-initializer-consumer>
			</umb-controller-host-provider>`,
		);
		consumer = element.getElementsByTagName(
			'umb-test-controller-host-initializer-consumer',
		)[0] as UmbTestControllerHostInitializerConsumerElement;
	});

	it('element is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbControllerHostProviderElement);
	});

	it('provides the context', async () => {
		await Promise.resolve();
		// Potentially we need to wait a bit here, cause the value might not be set already? as of the context consumption...
		expect(consumer).to.be.instanceOf(UmbTestControllerHostInitializerConsumerElement);
		expect(consumer.value).to.not.be.null;
		expect(consumer.value).to.equal(contextValue);
	});
});
