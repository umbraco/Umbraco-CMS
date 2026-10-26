import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import { UmbBlockWorkspaceHasContentPropertiesCondition } from './block-workspace-has-content-properties.condition.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-block-has-content-properties-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-block-has-content-properties-child')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockWorkspaceHasContentPropertiesCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let contentTypeHasProperties: UmbBooleanState<boolean | undefined>;
	let condition: UmbBlockWorkspaceHasContentPropertiesCondition;

	const createCondition = (onChange: () => void = () => {}) =>
		new UmbBlockWorkspaceHasContentPropertiesCondition(childElement, {
			host: childElement,
			config: { alias: 'Umb.Condition.BlockWorkspaceHasContentProperties' },
			onChange,
		});

	const nextFrame = () => new Promise((resolve) => requestAnimationFrame(resolve));

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		contentTypeHasProperties = new UmbBooleanState<boolean | undefined>(undefined);
		const workspaceContext = {
			IS_BLOCK_WORKSPACE_CONTEXT: true,
			getHostElement: () => hostElement,
			content: { structure: { contentTypeHasProperties: contentTypeHasProperties.asObservable() } },
		};
		const provider = new UmbContextProvider(hostElement, UMB_BLOCK_WORKSPACE_CONTEXT, workspaceContext as never);
		provider.hostConnected();
	});

	afterEach(() => {
		condition?.hostDisconnected();
		document.body.innerHTML = '';
	});

	it('is permitted when the content element type has properties', async () => {
		contentTypeHasProperties.setValue(true);
		condition = createCondition();

		await nextFrame();
		expect(condition.permitted).to.be.true;
	});

	it('is not permitted when the content element type has no properties', async () => {
		contentTypeHasProperties.setValue(false);
		condition = createCondition();

		await nextFrame();
		expect(condition.permitted).to.be.false;
	});

	it('is not permitted before the content element type has loaded', async () => {
		condition = createCondition();

		await nextFrame();
		expect(condition.permitted).to.be.false;
	});

	it('becomes permitted when properties appear on the content element type', async () => {
		contentTypeHasProperties.setValue(false);
		condition = createCondition();
		await nextFrame();
		expect(condition.permitted).to.be.false;

		contentTypeHasProperties.setValue(true);
		await nextFrame();
		expect(condition.permitted).to.be.true;
	});

	it('stops being permitted when the properties are removed', async () => {
		contentTypeHasProperties.setValue(true);
		condition = createCondition();
		await nextFrame();
		expect(condition.permitted).to.be.true;

		contentTypeHasProperties.setValue(false);
		await nextFrame();
		expect(condition.permitted).to.be.false;
	});
});
