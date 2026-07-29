import { UmbInteractionMemoryScopeContext } from './interaction-memory-scope.context.js';
import { UMB_INTERACTION_MEMORY_SCOPE_CONTEXT } from './interaction-memory-scope.context.token.js';
import { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { expect } from '@open-wc/testing';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-interaction-memory-scope-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbInteractionMemoryScopeContext', () => {
	let context: UmbInteractionMemoryScopeContext;

	beforeEach(() => {
		const hostElement = new UmbTestControllerHostElement();
		context = new UmbInteractionMemoryScopeContext(hostElement);
	});

	it('has an interactionMemory manager', () => {
		expect(context).to.have.property('interactionMemory').to.be.an.instanceOf(UmbInteractionMemoryManager);
	});

	it('provides itself under the scope context token', async () => {
		const hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		const consumerHost = new UmbTestControllerHostElement();
		hostElement.appendChild(consumerHost);

		const scopeContext = new UmbInteractionMemoryScopeContext(hostElement);

		const resolved = await new UmbContextConsumerController(consumerHost, UMB_INTERACTION_MEMORY_SCOPE_CONTEXT).asPromise();

		expect(resolved).to.equal(scopeContext);

		hostElement.remove();
	});
});
