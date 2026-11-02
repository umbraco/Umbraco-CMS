import { UmbEntityActionsBundleElement } from './entity-actions-bundle.element.js';
import { expect, fixture, fixtureCleanup, html, waitUntil } from '@open-wc/testing';
import { customElement, html as litHtml } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';

class UmbTestEntityActionApi extends UmbEntityActionBase<never> {
	override async execute() {}
}

const ENTITY_TYPE = 'test-entity-actions-bundle';
const GROUP = 'Umb.Test.EntityActionsBundle.Group';

@customElement('umb-test-entity-actions-bundle-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestEntityActionsBundleHostElement extends UmbLitElement {
	constructor() {
		super();
		const context = new UmbEntityContext(this);
		context.setEntityType(ENTITY_TYPE);
		context.setUnique('1234');
	}

	override render() {
		return litHtml`<umb-entity-actions-bundle label="Test"></umb-entity-actions-bundle>`;
	}
}

const action = (alias: string, weight: number, group?: string): ManifestEntityAction => ({
	type: 'entityAction',
	alias,
	name: alias,
	weight,
	group,
	forEntityTypes: [ENTITY_TYPE],
	api: UmbTestEntityActionApi,
	meta: { label: alias, icon: 'icon-add' } as never,
});

describe('UmbEntityActionsBundleElement', () => {
	const aliases = ['Umb.Test.EntityActionsBundle.Ungrouped', 'Umb.Test.EntityActionsBundle.Grouped'];

	beforeEach(() => {
		umbExtensionsRegistry.register({
			type: 'entityActionGroup',
			alias: GROUP,
			name: GROUP,
			weight: 1200,
			meta: { label: 'Test group' },
		});
		umbExtensionsRegistry.registerMany([action(aliases[0], 1000), action(aliases[1], 1, GROUP)]);
	});

	afterEach(() => {
		fixtureCleanup();
		umbExtensionsRegistry.unregisterMany([...aliases, GROUP]);
	});

	it('renders the first action of the ordered menu inline, following the action groups', async () => {
		const host = await fixture(html`<umb-test-entity-actions-bundle-host></umb-test-entity-actions-bundle-host>`);
		const bundle = host.shadowRoot!.querySelector('umb-entity-actions-bundle') as UmbEntityActionsBundleElement;
		expect(bundle).to.be.instanceOf(UmbEntityActionsBundleElement);

		await waitUntil(() => bundle.shadowRoot?.querySelector('uui-button[data-mark^="entity-action:"]'));
		const button = bundle.shadowRoot!.querySelector('uui-button[data-mark^="entity-action:"]')!;
		expect(button.getAttribute('data-mark')).to.equal(`entity-action:${aliases[1]}`);
	});
});
