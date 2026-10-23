import { UMB_ENTITY_WORKSPACE_CONTEXT } from '../../contexts/index.js';
import { UmbWorkspaceEntityActionMenuElement } from './workspace-entity-action-menu.element.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import type { UmbConditionConfigBase, UmbConditionControllerArguments } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

const ENTITY_TYPE = 'umb-test-workspace-entity-action-menu-entity';
const UNIQUE = 'test-unique';
const ACTION_ALIAS = 'Umb.Test.WorkspaceEntityActionMenu.Action';
const CONDITION_ALIAS = 'Umb.Test.WorkspaceEntityActionMenu.Condition';

const actionPermitted = new UmbBooleanState(false);

class UmbTestActionPermittedCondition extends UmbConditionBase<UmbConditionConfigBase> {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);
		this.observe(actionPermitted.asObservable(), (permitted) => (this.permitted = permitted), null);
	}
}

@customElement('umb-test-workspace-entity-action-menu-host')
class UmbTestWorkspaceEntityActionMenuHostElement extends UmbElementMixin(HTMLElement) {
	constructor() {
		super();
		const entityContext = new UmbEntityContext(this);
		entityContext.setEntityType(ENTITY_TYPE);
		entityContext.setUnique(UNIQUE);

		const workspaceContext = {
			unique: new UmbStringState(UNIQUE).asObservable(),
			getUnique: () => UNIQUE,
			getEntityType: () => ENTITY_TYPE,
			getHostElement: () => this,
		};
		new UmbContextProviderController(this, UMB_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
	}
}

describe('UmbWorkspaceEntityActionMenuElement', () => {
	let element: UmbWorkspaceEntityActionMenuElement;

	const hasDropdown = () => !!element.shadowRoot!.querySelector('umb-entity-actions-dropdown');

	const registerAction = (conditions?: Array<UmbConditionConfigBase>) =>
		umbExtensionsRegistry.register({
			type: 'entityAction',
			alias: ACTION_ALIAS,
			name: ACTION_ALIAS,
			forEntityTypes: [ENTITY_TYPE],
			conditions,
			meta: {},
		} as never);

	before(() => {
		umbExtensionsRegistry.register({
			type: 'condition',
			alias: CONDITION_ALIAS,
			name: CONDITION_ALIAS,
			api: UmbTestActionPermittedCondition,
		});
	});

	after(() => {
		umbExtensionsRegistry.unregister(CONDITION_ALIAS);
	});

	beforeEach(async () => {
		actionPermitted.setValue(false);
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

	it('renders the menu when the entity has a permitted action', async () => {
		registerAction();

		await waitUntil(hasDropdown, 'the menu did not render');
	});

	it('renders the menu only while an action is permitted', async () => {
		registerAction([{ alias: CONDITION_ALIAS }]);

		actionPermitted.setValue(true);
		await waitUntil(hasDropdown, 'the menu did not render once the action was permitted');

		actionPermitted.setValue(false);
		await waitUntil(() => !hasDropdown(), 'the menu was not removed once the action was denied');
	});

	it('removes the menu when the last action goes away', async () => {
		registerAction();
		await waitUntil(hasDropdown, 'the menu did not render');

		umbExtensionsRegistry.unregister(ACTION_ALIAS);

		await waitUntil(() => !hasDropdown(), 'the menu was not removed');
	});
});
