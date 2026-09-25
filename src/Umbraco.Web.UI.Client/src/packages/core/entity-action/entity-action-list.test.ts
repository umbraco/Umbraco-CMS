import { UmbEntityActionListElement } from './entity-action-list.element.js';
import type { ManifestEntityAction } from './entity-action.extension.js';
import { UmbEntityActionBase } from './entity-action-base.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement, html as litHtml } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import '../extension-registry/components/extension-with-api-slot/extension-with-api-slot.element.js';

@customElement('umb-test-entity-action-list-action')
class UmbTestEntityActionElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestEntityActionApi extends UmbEntityActionBase<never> {
	override async execute() {}
}

const ENTITY_TYPE = 'test-entity-action-list';

@customElement('umb-test-entity-action-list-host')
class UmbTestEntityActionListHostElement extends UmbLitElement {
	constructor() {
		super();
		const context = new UmbEntityContext(this);
		context.setEntityType(ENTITY_TYPE);
		context.setUnique('1234');
	}

	override render() {
		return litHtml`<umb-entity-action-list></umb-entity-action-list>`;
	}
}

function sleep(timeMs: number) {
	return new Promise((resolve) => setTimeout(resolve, timeMs));
}

function registerActions(actions: Array<{ alias: string; weight: number; group?: string }>) {
	umbExtensionsRegistry.registerMany(
		actions.map(
			(action): ManifestEntityAction => ({
				type: 'entityAction',
				alias: action.alias,
				name: action.alias,
				weight: action.weight,
				group: action.group,
				forEntityTypes: [ENTITY_TYPE],
				elementName: 'umb-test-entity-action-list-action',
				api: UmbTestEntityActionApi,
				meta: {},
			}),
		),
	);
}

/**
 * Reads the rendered list as a sequence of action aliases, with '|' marking a separator.
 * @param {UmbEntityActionListElement} element - The rendered entity action list.
 * @returns {Array<string>} The sequence.
 */
function readSequence(element: UmbEntityActionListElement): Array<string> {
	const slot = element.shadowRoot!.querySelector('umb-extension-with-api-slot')!;
	return Array.from(slot.shadowRoot!.children).map((child) =>
		child.getAttribute('role') === 'separator' ? '|' : (child as any).manifest.alias,
	);
}

describe('UmbEntityActionListElement', () => {
	let aliases: Array<string> = [];

	async function renderList(actions: Array<{ alias: string; weight: number; group?: string }>) {
		aliases = actions.map((a) => a.alias);
		registerActions(actions);
		const host = await fixture<UmbTestEntityActionListHostElement>(
			html`<umb-test-entity-action-list-host></umb-test-entity-action-list-host>`,
		);
		await sleep(100);
		return host.shadowRoot!.querySelector('umb-entity-action-list') as UmbEntityActionListElement;
	}

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(aliases);
	});

	it('is defined with its own instance', async () => {
		const element = await renderList([]);
		expect(element).to.be.instanceOf(UmbEntityActionListElement);
	});

	it('renders the test action element', async () => {
		const element = await renderList([{ alias: 'a', weight: 1 }]);
		const slot = element.shadowRoot!.querySelector('umb-extension-with-api-slot')!;
		expect(slot.shadowRoot!.firstElementChild).to.be.instanceOf(UmbTestEntityActionElement);
	});

	it('renders no separators when no action has a group', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 3 },
			{ alias: 'b', weight: 2 },
			{ alias: 'c', weight: 1 },
		]);
		expect(readSequence(element)).to.deep.equal(['a', 'b', 'c']);
	});

	it('renders a separator between adjacent actions of different groups', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 4, group: 'create' },
			{ alias: 'b', weight: 3, group: 'create' },
			{ alias: 'c', weight: 2, group: 'danger' },
			{ alias: 'd', weight: 1, group: 'danger' },
		]);
		expect(readSequence(element)).to.deep.equal(['a', 'b', '|', 'c', 'd']);
	});

	it('treats actions without a group as a group of their own', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 3, group: 'create' },
			{ alias: 'b', weight: 2 },
			{ alias: 'c', weight: 1, group: 'danger' },
		]);
		expect(readSequence(element)).to.deep.equal(['a', '|', 'b', '|', 'c']);
	});

	it('orders by weight and separates a group again when interleaved by another group', async () => {
		const element = await renderList([
			{ alias: 'a', weight: 3, group: 'create' },
			{ alias: 'b', weight: 2, group: 'danger' },
			{ alias: 'c', weight: 1, group: 'create' },
		]);
		expect(readSequence(element)).to.deep.equal(['a', '|', 'b', '|', 'c']);
	});

	it('keeps add-on groups contiguous when their weights are contiguous', async () => {
		const element = await renderList([
			{ alias: 'core.create', weight: 1200, group: 'create' },
			{ alias: 'core.publish', weight: 600, group: 'publishing' },
			{ alias: 'workflow.request', weight: 590, group: 'workflow' },
			{ alias: 'workflow.history', weight: 580, group: 'workflow' },
			{ alias: 'forms.entries', weight: 150, group: 'forms' },
			{ alias: 'core.delete', weight: 100, group: 'danger' },
		]);
		expect(readSequence(element)).to.deep.equal([
			'core.create',
			'|',
			'core.publish',
			'|',
			'workflow.request',
			'workflow.history',
			'|',
			'forms.entries',
			'|',
			'core.delete',
		]);
	});
});
