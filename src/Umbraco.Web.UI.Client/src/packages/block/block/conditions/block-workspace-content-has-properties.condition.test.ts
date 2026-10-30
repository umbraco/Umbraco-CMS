import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTENT_HAS_PROPERTIES_CONDITION_ALIAS } from './constants.js';
import { UmbBlockWorkspaceContentHasPropertiesCondition } from './block-workspace-content-has-properties.condition.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-block-content-has-properties-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-block-content-has-properties-child')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockWorkspaceContentHasPropertiesCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let contentTypeHasProperties: UmbBooleanState<boolean>;
	let condition: UmbBlockWorkspaceContentHasPropertiesCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		contentTypeHasProperties = new UmbBooleanState(false);
		const context = {
			IS_BLOCK_WORKSPACE_CONTEXT: true,
			getHostElement: () => hostElement,
			content: { structure: { contentTypeHasProperties: contentTypeHasProperties.asObservable() } },
		};
		const provider = new UmbContextProvider(hostElement, UMB_BLOCK_WORKSPACE_CONTEXT, context as any);
		provider.hostConnected();
	});

	afterEach(() => {
		condition?.destroy();
		document.body.innerHTML = '';
	});

	it('should not be permitted when the content element type has no properties', async () => {
		condition = new UmbBlockWorkspaceContentHasPropertiesCondition(childElement, {
			host: childElement,
			config: { alias: UMB_BLOCK_WORKSPACE_CONTENT_HAS_PROPERTIES_CONDITION_ALIAS },
			onChange: () => {},
		});

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
	});

	it('should be permitted when the content element type has properties', (done) => {
		contentTypeHasProperties.setValue(true);

		condition = new UmbBlockWorkspaceContentHasPropertiesCondition(childElement, {
			host: childElement,
			config: { alias: UMB_BLOCK_WORKSPACE_CONTENT_HAS_PROPERTIES_CONDITION_ALIAS },
			onChange: () => {
				expect(condition.permitted).to.be.true;
				done();
			},
		});
	});

	it('should update permitted when the content element type structure changes', (done) => {
		let callbackCount = 0;
		condition = new UmbBlockWorkspaceContentHasPropertiesCondition(childElement, {
			host: childElement,
			config: { alias: UMB_BLOCK_WORKSPACE_CONTENT_HAS_PROPERTIES_CONDITION_ALIAS },
			onChange: () => {
				callbackCount++;
				if (callbackCount === 1) {
					expect(condition.permitted).to.be.true;
					contentTypeHasProperties.setValue(false);
				} else if (callbackCount === 2) {
					expect(condition.permitted).to.be.false;
					done();
				}
			},
		});

		contentTypeHasProperties.setValue(true);
	});
});
