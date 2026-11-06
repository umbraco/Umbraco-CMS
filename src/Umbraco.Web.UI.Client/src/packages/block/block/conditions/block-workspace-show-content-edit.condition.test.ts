import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../workspace/block-workspace.context-token.js';
import { UMB_BLOCK_WORKSPACE_SHOW_CONTENT_EDIT_CONDITION_ALIAS } from './constants.js';
import { UmbBlockWorkspaceShowContentEditCondition } from './block-workspace-show-content-edit.condition.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-block-show-content-edit-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-block-show-content-edit-child')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockWorkspaceShowContentEditCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let contentTypeHasProperties: UmbBooleanState<boolean>;
	let forceHideContentEditorInOverlay: UmbBooleanState<boolean>;
	let condition: UmbBlockWorkspaceShowContentEditCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		contentTypeHasProperties = new UmbBooleanState(false);
		forceHideContentEditorInOverlay = new UmbBooleanState(false);
		// Same rule as UmbBlockWorkspaceContext.showContentEdit
		const context = {
			IS_BLOCK_WORKSPACE_CONTEXT: true,
			getHostElement: () => hostElement,
			showContentEdit: mergeObservables(
				[contentTypeHasProperties.asObservable(), forceHideContentEditorInOverlay.asObservable()],
				([hasProperties, forceHide]) => hasProperties === true && forceHide === false,
			),
		};
		const provider = new UmbContextProvider(hostElement, UMB_BLOCK_WORKSPACE_CONTEXT, context as any);
		provider.hostConnected();
	});

	afterEach(() => {
		condition?.destroy();
		document.body.innerHTML = '';
	});

	function createCondition(onChange: () => void = () => {}) {
		condition = new UmbBlockWorkspaceShowContentEditCondition(childElement, {
			host: childElement,
			config: { alias: UMB_BLOCK_WORKSPACE_SHOW_CONTENT_EDIT_CONDITION_ALIAS },
			onChange,
		});
	}

	it('should not be permitted when the content element type has no properties', async () => {
		createCondition();
		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
	});

	it('should not be permitted when the block type hides the content editor in the overlay', async () => {
		contentTypeHasProperties.setValue(true);
		forceHideContentEditorInOverlay.setValue(true);
		createCondition();
		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
	});

	it('should be permitted when the content element type has properties', (done) => {
		contentTypeHasProperties.setValue(true);
		createCondition(() => {
			expect(condition.permitted).to.be.true;
			done();
		});
	});

	it('should update permitted when the content element type structure changes', (done) => {
		let callbackCount = 0;
		createCondition(() => {
			callbackCount++;
			if (callbackCount === 1) {
				expect(condition.permitted).to.be.true;
				contentTypeHasProperties.setValue(false);
			} else if (callbackCount === 2) {
				expect(condition.permitted).to.be.false;
				done();
			}
		});
		contentTypeHasProperties.setValue(true);
	});
});
