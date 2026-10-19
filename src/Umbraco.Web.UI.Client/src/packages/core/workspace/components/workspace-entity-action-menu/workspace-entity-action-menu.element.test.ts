import { UMB_ENTITY_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { UmbWorkspaceEntityActionMenuElement } from './workspace-entity-action-menu.element.js';
import { expect, waitUntil } from '@open-wc/testing';
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

describe('UmbWorkspaceEntityActionMenuElement', () => {
	async function render(entityType: string) {
		const host = new UmbTestHostElement();
		new UmbContextProviderController(
			host,
			UMB_ENTITY_WORKSPACE_CONTEXT,
			new UmbTestEntityWorkspaceContext(host, entityType) as any,
		);
		const element = new UmbWorkspaceEntityActionMenuElement();
		host.appendChild(element);
		document.body.appendChild(host);
		await element.updateComplete;
		return element;
	}

	const dropdown = (element: UmbWorkspaceEntityActionMenuElement) =>
		element.shadowRoot?.querySelector('umb-entity-actions-dropdown');

	it('renders nothing when no entity actions are registered for the entity type', async () => {
		const element = await render('umb-test-entity-without-actions');
		await new Promise((resolve) => setTimeout(resolve, 50));
		expect(dropdown(element)).to.be.null;
	});

	it('renders the dropdown when an entity action is registered for the entity type', async () => {
		umbExtensionsRegistry.register({
			type: 'entityAction',
			alias: 'Umb.Test.WorkspaceEntityActionMenu.Action',
			name: 'Test Action',
			forEntityTypes: ['umb-test-entity-with-actions'],
			meta: {},
		} as any);

		const element = await render('umb-test-entity-with-actions');
		await waitUntil(() => dropdown(element), 'dropdown should render');
		expect(dropdown(element)).to.not.be.null;

		umbExtensionsRegistry.unregister('Umb.Test.WorkspaceEntityActionMenu.Action');
	});
});
