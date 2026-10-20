import { UMB_ENTITY_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { UmbWorkspaceEntityActionMenuElement } from './workspace-entity-action-menu.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-workspace-entity-action-menu-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/** Satisfies UMB_ENTITY_WORKSPACE_CONTEXT's discriminator (`getUnique` present). */
class UmbTestEntityWorkspaceContext {
	readonly workspaceAlias = 'Umb.Test.Workspace';
	#unique = new UmbStringState<string | null>(null);
	readonly unique = this.#unique.asObservable();

	constructor(
		private host: HTMLElement,
		private entityType: string,
	) {}

	getHostElement() {
		return this.host;
	}

	getUnique() {
		return this.#unique.getValue();
	}

	getEntityType() {
		return this.entityType;
	}
}

const TEST_ACTION_ALIAS = 'Umb.Test.WorkspaceEntityActionMenu.Action';

describe('UmbWorkspaceEntityActionMenuElement', () => {
	afterEach(() => {
		umbExtensionsRegistry.unregister(TEST_ACTION_ALIAS);
	});

	async function render(entityType: string) {
		// fixture() removes the host from the DOM after each test.
		const host = await fixture<UmbTestHostElement>(
			html`<umb-test-workspace-entity-action-menu-host></umb-test-workspace-entity-action-menu-host>`,
		);
		new UmbContextProviderController(
			host,
			UMB_ENTITY_WORKSPACE_CONTEXT,
			new UmbTestEntityWorkspaceContext(host, entityType) as any,
		);
		const element = new UmbWorkspaceEntityActionMenuElement();
		host.appendChild(element);
		await element.updateComplete;
		return element;
	}

	// Assert on a boolean rather than the element itself, so a failure reports cleanly
	// instead of the test runner hanging while it serializes the element.
	const hasDropdown = (element: UmbWorkspaceEntityActionMenuElement) =>
		!!element.shadowRoot?.querySelector('umb-entity-actions-dropdown');

	it('renders nothing when no entity actions are registered for the entity type', async () => {
		const element = await render('umb-test-entity-without-actions');
		await new Promise((resolve) => setTimeout(resolve, 50));
		expect(hasDropdown(element), 'dropdown should not render').to.be.false;
	});

	it('renders the dropdown when an entity action is registered for the entity type', async () => {
		umbExtensionsRegistry.register({
			type: 'entityAction',
			alias: TEST_ACTION_ALIAS,
			name: 'Test Action',
			forEntityTypes: ['umb-test-entity-with-actions'],
			meta: {},
		} as any);

		const element = await render('umb-test-entity-with-actions');
		await waitUntil(() => hasDropdown(element), 'dropdown should render');
		expect(hasDropdown(element)).to.be.true;
	});
});
