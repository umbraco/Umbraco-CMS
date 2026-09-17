import { UmbFolderWorkspaceEditorElement } from './folder-workspace-editor.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('test-folder-workspace-editor-host')
class UmbTestFolderWorkspaceEditorHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * The parts of an entity detail workspace context that the folder workspace editor observes.
 */
class UmbTestEntityDetailWorkspaceContext {
	readonly IS_ENTITY_DETAIL_WORKSPACE_CONTEXT = true;

	#hostElement: Element;

	#entityType = new UmbStringState<string | undefined>(undefined);
	readonly entityType = this.#entityType.asObservable();

	#forbidden = new UmbBooleanState<boolean>(false);
	readonly forbidden = { isOn: this.#forbidden.asObservable() };

	#loading = new UmbBooleanState<boolean>(false);
	readonly loading = { isOn: this.#loading.asObservable() };

	constructor(hostElement: Element, entityType: string) {
		this.#hostElement = hostElement;
		this.#entityType.setValue(entityType);
	}

	getHostElement() {
		return this.#hostElement;
	}

	setForbidden(value: boolean) {
		this.#forbidden.setValue(value);
	}

	setLoading(value: boolean) {
		this.#loading.setValue(value);
	}
}

describe('UmbFolderWorkspaceEditorElement', () => {
	let context: UmbTestEntityDetailWorkspaceContext;
	let element: UmbFolderWorkspaceEditorElement;

	const forbiddenView = () => element.shadowRoot!.querySelector('umb-entity-detail-forbidden');
	const workspaceEditor = () => element.shadowRoot!.querySelector('umb-workspace-editor');

	// The workspace context is consumed asynchronously, so it arrives after the element's first
	// render. Yield a task to let it land, then await the render its state changes schedule.
	const settle = async () => {
		await aTimeout(0);
		await element.updateComplete;
	};

	beforeEach(async () => {
		const host = await fixture<UmbTestFolderWorkspaceEditorHostElement>(
			html`<test-folder-workspace-editor-host></test-folder-workspace-editor-host>`,
		);

		context = new UmbTestEntityDetailWorkspaceContext(host, 'element-folder');
		new UmbContextProvider(
			host,
			UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT,
			context as unknown as typeof UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT.TYPE,
		).hostConnected();

		element = document.createElement('umb-folder-workspace-editor') as UmbFolderWorkspaceEditorElement;
		host.appendChild(element);
		await settle();
	});

	describe('when access to the folder is permitted', () => {
		it('renders the workspace editor', () => {
			expect(workspaceEditor()).to.exist;
		});

		it('does not render the forbidden view', () => {
			expect(forbiddenView()).to.not.exist;
		});
	});

	describe('when access to the folder is forbidden', () => {
		beforeEach(async () => {
			context.setForbidden(true);
			await settle();
		});

		it('renders the forbidden view', () => {
			expect(forbiddenView()).to.exist;
		});

		it('names the entity type on the forbidden view', () => {
			expect(forbiddenView()!.getAttribute('entity-type')).to.equal('element-folder');
		});

		it('does not render the workspace editor', () => {
			expect(workspaceEditor()).to.not.exist;
		});
	});

	describe('while the folder is still loading', () => {
		beforeEach(async () => {
			context.setLoading(true);
			context.setForbidden(true);
			await settle();
		});

		it('does not render the forbidden view until loading has settled', () => {
			expect(forbiddenView()).to.not.exist;
		});

		it('renders the forbidden view once loading has settled', async () => {
			context.setLoading(false);
			await settle();

			expect(forbiddenView()).to.exist;
		});
	});
});
