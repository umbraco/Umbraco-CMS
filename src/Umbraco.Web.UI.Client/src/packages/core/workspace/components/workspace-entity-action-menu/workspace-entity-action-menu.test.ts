import { UmbWorkspaceEntityActionMenuElement } from './workspace-entity-action-menu.element.js';
import { aTimeout, expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const ENTITY_TYPE = 'umb-test-workspace-entity-action-menu-entity';
const ACTION_ALIAS = 'Umb.Test.WorkspaceEntityActionMenu.Action';

@customElement('umb-test-workspace-entity-action-menu-host')
class UmbTestWorkspaceEntityActionMenuHostElement extends UmbElementMixin(HTMLElement) {
	constructor() {
		super();
		const entityContext = new UmbEntityContext(this);
		entityContext.setEntityType(ENTITY_TYPE);
		entityContext.setUnique('test-unique');
	}
}

describe('UmbWorkspaceEntityActionMenuElement', () => {
	let element: UmbWorkspaceEntityActionMenuElement;

	const hasDropdown = () => !!element.shadowRoot!.querySelector('umb-entity-actions-dropdown');

	const registerAction = () =>
		umbExtensionsRegistry.register({
			type: 'entityAction',
			alias: ACTION_ALIAS,
			name: ACTION_ALIAS,
			forEntityTypes: [ENTITY_TYPE],
			meta: {},
		} as never);

	beforeEach(async () => {
		const host = await fixture<UmbTestWorkspaceEntityActionMenuHostElement>(
			html`<umb-test-workspace-entity-action-menu-host>
				<umb-workspace-entity-action-menu></umb-workspace-entity-action-menu>
			</umb-test-workspace-entity-action-menu-host>`,
		);
		element = host.querySelector('umb-workspace-entity-action-menu')!;
	});

	afterEach(() => {
		umbExtensionsRegistry.unregister(ACTION_ALIAS);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbWorkspaceEntityActionMenuElement);
	});

	it('renders no menu when the entity has no actions', async () => {
		await aTimeout(0);

		expect(hasDropdown()).to.be.false;
	});

	it('renders the menu when the entity has an action', async () => {
		registerAction();

		await waitUntil(hasDropdown, 'the menu did not render');
	});

	it('removes the menu when the last action goes away', async () => {
		registerAction();
		await waitUntil(hasDropdown, 'the menu did not render');

		umbExtensionsRegistry.unregister(ACTION_ALIAS);

		await waitUntil(() => !hasDropdown(), 'the menu was not removed');
	});
});
